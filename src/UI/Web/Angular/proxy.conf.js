const PROXY_CONFIG = {
  "/api": {
    "target": process.env["API_URL"] || "http://localhost:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
};

module.exports = PROXY_CONFIG;
