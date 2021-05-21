using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Authentication
{
    /// <summary>
    /// Provides background job which runs every minute to remove expired refresh tokens in memory
    /// </summary>
    public class JwtRefreshTokenCache : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IJwtAuthManager _jwtAuthManager;

        public JwtRefreshTokenCache(IJwtAuthManager jwtAuthManager)
        {
            _jwtAuthManager = jwtAuthManager;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            // remove expired refresh tokens from cache every minute
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));//Generate new timer that will work every minute
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked by the timer
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            _jwtAuthManager.RemoveExpiredRefreshTokens(DateTime.Now);//removes the expired tokens
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
