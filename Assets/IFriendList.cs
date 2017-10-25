namespace Com.Hypester.DM3
{
    public interface IEntryList
    {
        void Selected(UIEntry entry, bool yes);
        void EntryPrimaryAction(UIEntry entry);
    }
}