namespace TMP.WORK.AramisChetchiki
{
    using System.ComponentModel;
    using System.Linq;
    using GongSolutions.Wpf.DragDrop;
    using TMP.Shared;

    public class GroupedDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            // Call default DragOver method, cause most stuff should work by default
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            if (dropInfo.TargetGroup == null)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }

            dropInfo.EffectText = "Hi. Test";
        }

        public void Drop(IDropInfo dropInfo)
        {
            // The default drop handler don't know how to set an item's group. You need to explicitly set the group on the dropped item like this.
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            // Now extract the dragged group items and set the new group (target)
            System.Collections.Generic.List<PlusPropertyDescriptor> data = DefaultDropHandler.ExtractData(dropInfo.Data).OfType<PlusPropertyDescriptor>().ToList();
            foreach (PlusPropertyDescriptor groupedItem in data)
            {
                // groupedItem.GroupName = dropInfo.TargetGroup.Name.ToString();
            }

            // Changing group data at runtime isn't handled well: force a refresh on the collection view.
            if (dropInfo.TargetCollection is ICollectionView)
            {
                ((ICollectionView)dropInfo.TargetCollection).Refresh();
            }
        }
    }
}
