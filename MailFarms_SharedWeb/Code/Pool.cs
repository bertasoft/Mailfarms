using System.Buffers;

namespace MailFarms_SharedWeb.Code
{
    public static class Pool<T>
    {
        public static T[] SpaceGet(int minLenght)
        {
            return ArrayPool<T>.Shared.Rent(minLenght);
        }

        public static void SpaceReturn(T[] returned)
        {
            // Imposta `clearArray` su true se `T` è un reference type
            bool clearArray = !typeof(T).IsValueType;

            ArrayPool<T>.Shared.Return(returned, clearArray); //a true perché se fa riferimento a qualche oggetto ref (classi) la memoria non viene più rilasciata
        }
    }
}
