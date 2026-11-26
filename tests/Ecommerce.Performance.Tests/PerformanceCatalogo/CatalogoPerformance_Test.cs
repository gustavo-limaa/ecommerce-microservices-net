using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Ecommerce.Performance.Tests.PerformanceCatalogo
{
    public sealed class CatalogoPerformanceTests : PerformanceTestBase
    {
        public CatalogoPerformanceTests(ITestOutputHelper output)
            : base(new Uri("http://localhost:5000"), output)
        {
        }

        [Fact]
        public async Task GetCatalogo_DeveResponderDentroDoLimite()
        {
            await MedirEndpointGetAsync("/api/produtos", iteracoes: 1
                , limiteMaximoMediaMs: 500);
        }

        [Fact]
        public async Task GetCatalogoPorId_DeveResponderDentroDoLimite()
        {
            await MedirEndpointGetAsync($"/api/categorias", iteracoes: 15, limiteMaximoMediaMs: 500);
        }
    }
}