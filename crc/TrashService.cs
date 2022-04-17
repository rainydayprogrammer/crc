using System;
using System.Collections.Generic;

public class TrashService
{
    public List<ICalcItem> TrashItems;

    public TrashService()
    {
        List<ICalcItem> trashItems = new List<ICalcItem>();
        TrashItems = trashItems;
    }



    public void BackToCurrent()
    {
        throw new NotImplementedException();
    }

    public void EmptyTrash()
    {
        throw new NotImplementedException();
    }

}