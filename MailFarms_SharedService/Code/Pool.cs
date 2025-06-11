using System.Buffers;

namespace MailFarms_SharedService.Code
{
    public static class Pool<T>
    {
        public static T[] SpaceGet(int minLenght)
        {
            return ArrayPool<T>.Shared.Rent(minLenght);
        }

        public static void SpaceReturn(T[] returned)
        {
            ArrayPool<T>.Shared.Return(returned);
        }
    }
}
