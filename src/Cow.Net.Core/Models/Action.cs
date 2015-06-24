namespace Cow.Net.Core.Models
{
    public enum Action
    {
        connected,        
        peerGone,
        newList,        
        syncinfo,
        wantedList,
        missingRecords,
        updatedRecord,
        Unknown
    }
}
