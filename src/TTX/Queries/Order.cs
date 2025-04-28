namespace TTX.Queries
{
    public struct Order<T>
    {
        public required T By { get; set; }
        public required OrderDirection Dir { get; set; }
    }
}