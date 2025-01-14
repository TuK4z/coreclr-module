namespace AltV.Net.Elements.Entities
{
    internal interface IInternalBaseObject
    {
        bool Exists { set; }

        void ClearData();
        
        void SetCached(IntPtr pointer);
    }
}