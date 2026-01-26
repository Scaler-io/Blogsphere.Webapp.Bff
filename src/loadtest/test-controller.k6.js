import http from "k6/http";
import { check, sleep } from "k6";
import { Trend, Rate } from "k6/metrics";

const baseUrl = __ENV.BASE_URL || "http://localhost:8003";
const token = __ENV.ACCESS_TOKEN || "";

// Your controller route is: /api/v{version}/[controller]
// and your controller name is TestController => /test
const url = `${baseUrl}/api/v1/test`;

const latency = new Trend("latency_ms");
const failureRate = new Rate("failures");

export const options = {
  stages: [
    { duration: "10s", target: 10 },
    { duration: "30s", target: 50 },
    { duration: "10s", target: 0 },
  ],
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<1000"],
  },
};

export default function () {
  const headers = {
    // controller is [Authorize], so token is required
    Authorization: token ? `Bearer ${token}` : "",
    "api-version": "v1",
    CorrelationId: `${__VU}-${__ITER}`,
  };

  const res = http.get(url, { headers });
  latency.add(res.timings.duration);
  failureRate.add(res.status >= 400);

  check(res, {
    "status is 200": (r) => r.status === 200,
  });

  sleep(1);
}

