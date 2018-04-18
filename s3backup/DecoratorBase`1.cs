namespace S3Backup
{
    public class DecoratorBase<TComponentInterface>
    {
        public DecoratorBase(TComponentInterface component)
        {
            Inner = component;
        }

        protected TComponentInterface Inner { get; }

        public TComponentInterface GetComponent()
        {
            if (Inner is DecoratorBase<TComponentInterface> decorator)
            {
                return decorator.GetComponent();
            }

            return Inner;
        }
    }
}
