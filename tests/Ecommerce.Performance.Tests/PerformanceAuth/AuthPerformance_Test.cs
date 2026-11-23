using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Ecommerce.Performance.Tests.PerformanceAuth
{
    public sealed class AuthPerformanceTests : PerformanceTestBase
    {
        public AuthPerformanceTests(ITestOutputHelper output)
            : base(new Uri("http://localhost:5000"), output)
        {
        }

        [Fact]
        public async Task HealthLive_DeveResponderRapido()
        {
            await MedirEndpointGetAsync("/health/live", iteracoes: 15, limiteMaximoMediaMs: 200);
        }

        [Fact]
        public async Task HealthReady_DeveResponderRapido()
        {
            await MedirEndpointGetAsync("/health/ready", iteracoes: 15, limiteMaximoMediaMs: 200);
        }
    }
}