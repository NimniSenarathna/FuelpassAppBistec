// next.config.js
module.exports = {
    async rewrites() {
      return [
        {
            source: '/api/Vehicle',
          destination: 'http://localhost:7222/api/:path*', 
        },
      ];
    },
  };
  