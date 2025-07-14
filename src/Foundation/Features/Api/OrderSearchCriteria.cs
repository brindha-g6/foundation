
namespace Foundation.Features.Api
{
    internal class OrderSearchCriteria : OrderSearchFilter
    {
        public string OrderNumber { get; set; }
        public int RecordsToRetrieve { get; set; }
    }
}