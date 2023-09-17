namespace ProjectManagement.Application.Contracts.Class
{
    public class UpdateTable
    {
        public UpdateTable()
        {
            Items = new List<UpdateTableItem>();
            IsDifferent = false;
        }
        public bool IsDifferent { get; set; }
        public string Id { get; set; }
        public List<UpdateTableItem> Items { get; set; }
    }

    public  class UpdateTableItem
    {
        public UpdateTableItem(string oldItem, string newItem, string title)
        {
            OldItem = oldItem;
            NewItem = newItem;
            Title = title;
        }

        public string Title { get; set; }
        public string OldItem { get; set; }
        public string NewItem { get; set; }
    }
}
