using EPiServer.Shell.Navigation;

namespace Flinn_Migration.Business
{
    public class AdminToolMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            return new[]
            {
               
                new UrlMenuItem("Order Status Sync", "/global/cms/admin/tools/Ordersync", "/Ordersync"),
                new UrlMenuItem("Inventory Sync", "/global/cms/admin/tools/InventorySync", "/InventorySync")


            };

        }
    }
}
