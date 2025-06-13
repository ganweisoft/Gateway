namespace GWDataCenter
{
    interface ICanReset
    {
        bool ResetWhenDBChanged(params object[] o);
    }
}
