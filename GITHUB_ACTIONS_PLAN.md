# GitHub Actions plan (Azure DevOps parity)

This document proposes a GitHub Actions CI/CD setup for **this repository** (`Blogsphere.Webapp.Bff`) that mirrors the tasks and stage flow used in the Azure DevOps pipelines in [`Blogsphere.Api.gateway/devops`](https://github.com/Scaler-io/Blogsphere.Api.gateway/tree/main/devops).

## Goals

- **Parity with the upstream Azure pipeline stages**:
    - **Build**
    - **CodeAnalysis**
    - **GitHubRelease** (only on `main`, not for PRs)
    - **DockerBuildAndPush** (only on `main`, not for PRs; tag = release tag)
- **Same trigger behavior**:
    - CI on push to: `main`, `feature/**`, `hotfix/**`
    - PR validation for PRs targeting: `main`
- **Same “structure idea”** as the Azure setup: one orchestrator pipeline that calls “build / analysis / deploy” units.
- **Repository-appropriate paths** (this repo is not `Blogsphere.Api.Gateway`):
    - Solution: `src/Blogsphere.Webapp.Bff.sln`
    - API Dockerfile: `src/Blogsphere.Webapp.Bff.API/Dockerfile`
    - Docker build context: `src/` (matches how the Dockerfile copies project paths)
- **No secrets in git**; everything sensitive comes from GitHub Secrets / Environments.

## Non-goals (for this first iteration)

- Kubernetes/AKS deployment, Helm, Terraform, etc. (not present in the referenced Azure pipeline)
- Release notes automation beyond “commit-based changelog” equivalent
- Multi-arch images or SBOM/signing (can be added later)

## What the referenced Azure DevOps pipeline does (baseline)

From `Blogsphere.Api.gateway/devops/pipline.yml` and templates:

- **Build stage**
    - Install .NET SDK
    - `dotnet restore`
    - `dotnet build`
    - `dotnet publish` to an artifacts directory + zip
    - Publish build artifacts
- **CodeAnalysis stage**
    - PowerShell scripts that:
        - run `dotnet list package --vulnerable`
        - run “code quality” checks via `dotnet build` + parse warnings/errors
        - run `dotnet list package` and `--outdated`
        - compute simple code metrics (count C# files + lines)
        - generate a CSV report artifact
- **GitHubRelease stage**
    - Only on successful builds on `main` and not for PRs
    - Generate a release tag `v1.0.$(Build.BuildId)`
    - Create a GitHub Release using that tag
- **DockerBuildAndPush stage**
    - Only on successful builds on `main` and not for PRs
    - Docker login
    - Docker build using a Dockerfile path + build context
    - Docker push with the same release tag

## Proposed GitHub Actions structure

GitHub Actions doesn’t support “templates” the same way Azure Pipelines does, but we can mirror the structure using **reusable workflows** (recommended) or **composite actions**.

### Option A (recommended): reusable workflows (closest to “templates”)

- **Orchestrator workflow**: `.github/workflows/pipeline.yml`
- **Reusable workflows**:
    - `.github/workflows/build.yml` (called via `workflow_call`)
    - `.github/workflows/code-analysis.yml`
    - `.github/workflows/github-release.yml`
    - `.github/workflows/docker-build-push.yml`

This preserves the “one pipeline that calls 3-4 units” model, similar to `devops/pipline.yml` calling `./build/build.yml`, `./analysis/code-analysis.yml`, etc.

### Option B: single workflow file (one “script” only)

If you strictly want **one** workflow file, we can implement everything as jobs in:

- `.github/workflows/pipeline.yml`

This is simpler, but slightly less “same structure” than the Azure template approach.

### Decision (proposed)

Use **Option A** (reusable workflows) to keep the Azure-like separation **and** still run as a single orchestrated pipeline.

## Triggers (parity with Azure)

In `.github/workflows/pipeline.yml`:

- **push** on:
    - `main`
    - `feature/**`
    - `hotfix/**`
- **pull_request** targeting:
    - `main`

## Stages/jobs and ordering

GitHub Actions “stages” map to **jobs** with `needs:` dependencies.

### 1) Build job

- **Runs on**: `ubuntu-latest`
- **Steps**:
    - Checkout
    - Setup .NET SDK (`8.0.x`)
    - `dotnet restore "src/Blogsphere.Webapp.Bff.sln"`
    - `dotnet build "src/Blogsphere.Webapp.Bff.sln" -c Release --no-restore`
    - `dotnet test "src/Blogsphere.Webapp.Bff.sln" -c Release --no-build`
        - Note: Azure pipeline didn’t explicitly run tests, but this repo contains test projects; PR validation is materially better with tests.
    - `dotnet publish` (publish the API project) into a build output folder
    - Zip the publish output (to mirror Azure’s `zipAfterPublish`)
    - Upload artifact (equivalent of `PublishBuildArtifacts@1`)

**Artifact name proposal**: `build-output`

**Build configuration**: `Release`

### 2) Code analysis job

- **Depends on**: Build (to mirror stage ordering; job will still checkout code)
- **Runs on**: `ubuntu-latest` with `pwsh`
- **Steps (parity with Azure scripts)**:
    - Checkout
    - Setup .NET SDK
    - Run vulnerable package scan:
        - Prefer: `dotnet list "src/Blogsphere.Webapp.Bff.sln" package --vulnerable`
        - Keep output in logs
    - Run code quality check:
        - `dotnet build ...` and optionally parse warnings count
    - Run dependency checks:
        - `dotnet list ... package`
        - `dotnet list ... package --outdated`
    - Code metrics:
        - Count `src/**/*.cs` files + total lines (replicate Azure metric)
    - Generate a CSV report and upload artifact

**Artifact name proposal**: `code-analysis-report`

### 3) GitHub release job

- **Depends on**: Code analysis (mirrors Azure which depends on Build → CodeAnalysis → Release)
- **Condition** (parity with Azure):
    - Only when:
        - workflow succeeded
        - event is not PR
        - branch is `main`
- **Permissions**:
    - `contents: write` (required to create releases/tags with `GITHUB_TOKEN`)
- **Release tag** (Azure parity):
    - `v1.0.${{ github.run_number }}` (closest analog to `$(Build.BuildId)`)
- **Outputs**:
    - Expose `RELEASE_TAG` as a job output so the Docker job uses the same tag
- **Implementation approach**:
    - Create an annotated tag + release via `gh release create` or a standard release action.
    - Changelog mode:
        - Azure used “commit based comparing to last release”.
        - GitHub can generate notes automatically (`--generate-notes`) or we can implement a commit-based notes section.

### 4) Docker build & push job

- **Depends on**: GitHub release (for the tag output)
- **Condition**: same as release job (only on `main`, not PR)
- **Docker registry**:
    - Azure template logs into Docker Hub via a service connection.
    - GitHub Actions equivalent uses `docker/login-action`.
- **Build**:
    - Dockerfile: `src/Blogsphere.Webapp.Bff.API/Dockerfile`
    - Context: `src`
- **Push**:
    - Tag: `${{ needs.github_release.outputs.RELEASE_TAG }}`
    - Image name: configurable via repo variable

## Variables / Secrets mapping (Azure variable group → GitHub)

Azure uses `variables: - group: Blogsphere.Api.Gateway.Group`. GitHub has:

- **Repository Variables** (non-sensitive): Settings → Secrets and variables → Actions → Variables
- **Repository Secrets** (sensitive): Settings → Secrets and variables → Actions → Secrets
- **Environments** (optional): for per-environment secrets/approvals (e.g., `prod`)

### Proposed GitHub variables (non-secret)

- **`DOTNET_VERSION`**: `8.0.x`
- **`BUILD_CONFIGURATION`**: `Release`
- **`SOLUTION_PATH`**: `src/Blogsphere.Webapp.Bff.sln`
- **`API_PROJECT_PATH`**: `src/Blogsphere.Webapp.Bff.API/Blogsphere.Webapp.Bff.API.csproj`
- **`DOCKERFILE_PATH`**: `src/Blogsphere.Webapp.Bff.API/Dockerfile`
- **`DOCKER_CONTEXT`**: `src`
- **`IMAGE_NAME`**: e.g. `scalerio/blogsphere-webapp-bff` (example; set to your actual Docker Hub repo)

### Proposed GitHub secrets (sensitive)

For Docker Hub:

- **`DOCKERHUB_USERNAME`**
- **`DOCKERHUB_TOKEN`** (a PAT/access token; avoid using your password)

For GitHub Release:

- **No extra secret needed** if using built-in `GITHUB_TOKEN`
    - Ensure workflow permissions allow contents write (repo setting or workflow `permissions:`)

## Permissions & security controls

- Default to **least privilege**.
- Pipeline orchestrator can declare:
    - `permissions: read-all`
- Release workflow/job overrides to:
    - `contents: write`
- Docker push uses Docker Hub creds from Secrets; do not print them.

## Proposed workflow behavior details

### Concurrency

To avoid multiple `main` releases in parallel:

- Add `concurrency` on `main` runs (group by branch), e.g. “pipeline-main”.

### Artifact retention

- Keep build artifacts for a short time (e.g. 7–14 days).
- Keep analysis report similarly.

### Caching

Optional but recommended:

- Cache NuGet packages (`~/.nuget/packages`) using `actions/cache`.
- Cache Docker buildx layers (or use registry cache).

## YAML shape (high-level sketch)

Below is a **sketch** (not final code) of the orchestrator flow to show the mapping.

```yaml
on:
    push:
        branches: [main, "feature/**", "hotfix/**"]
    pull_request:
        branches: [main]

jobs:
    build:
        uses: ./.github/workflows/build.yml

    code_analysis:
        needs: [build]
        uses: ./.github/workflows/code-analysis.yml

    github_release:
        needs: [code_analysis]
        if: github.event_name != 'pull_request' && github.ref == 'refs/heads/main'
        uses: ./.github/workflows/github-release.yml

    docker_build_push:
        needs: [github_release]
        if: github.event_name != 'pull_request' && github.ref == 'refs/heads/main'
        uses: ./.github/workflows/docker-build-push.yml
        with:
            tag: ${{ needs.github_release.outputs.release_tag }}
```

## Implementation checklist (what I will build after you approve this plan)

- Add `.github/workflows/pipeline.yml` orchestrator
- Add reusable workflows:
    - `.github/workflows/build.yml`
    - `.github/workflows/code-analysis.yml`
    - `.github/workflows/github-release.yml`
    - `.github/workflows/docker-build-push.yml`
- Ensure the workflows:
    - run on PRs (build + analysis)
    - create GitHub Release only on `main` pushes
    - build+push docker image only on `main` pushes
    - pass the release tag output to docker job
- Document required repo variables/secrets in a short “CI/CD setup” section in `README.md` (optional, only if you want)

## Open items you may want to decide before implementation

- **Docker image name**: confirm your desired Docker Hub repo (e.g., `org/name`)
- **Release versioning**:
    - keep `v1.0.<run_number>` (Azure parity), or
    - derive from `Directory.Build.props <Version>` + run number
- **Tests**:
    - run `dotnet test` on PRs (recommended) or keep strict parity and skip tests
