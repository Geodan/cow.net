namespace Cow.Net.Core.Models
{
    public enum Action
    {
        connected,    
        command,
        peerGone,
        newList,        
        syncinfo,
        wantedList,
        missingRecord,
        updatedRecord,
        requestedRecord,
        Unknown
    }
}
