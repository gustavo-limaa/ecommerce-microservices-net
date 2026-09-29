using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Setup
{
    [CollectionDefinition("PedidoCollection")]
    public class PedidoCollection : ICollectionFixture<PedidoWebApplicationFactory>
    {
    }
}