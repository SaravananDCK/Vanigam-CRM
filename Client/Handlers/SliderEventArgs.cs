namespace Vanigam.CRM.Client.Handlers
{
    public class SliderEventArgs<T> where T : class
    {
        public T Object { get; set; }
        public int Index { get; set; }
        public float? Value { get; set; }
    }
}

