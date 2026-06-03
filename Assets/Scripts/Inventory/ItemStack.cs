// A slot's content: an Item + a count. Pure value type.
//
// An "empty" stack is item == null OR count <= 0. Use ItemStack.Empty as the
// canonical empty value. Inventory code treats both forms as equivalent so
// callers do not have to be paranoid about which one they are looking at.
[System.Serializable]
public struct ItemStack
{
    public Item item;
    public int  count;

    public bool IsEmpty => item == null || count <= 0;
    public static ItemStack Empty => default;

    public ItemStack(Item item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
