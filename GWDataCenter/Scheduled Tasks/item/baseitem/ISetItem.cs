namespace GWDataCenter
{
    public interface ISetItem
    {
        bool DoSetItem();
        bool m_bTemporarilyBreak
        {
            get;
            set;
        }
    }
}
