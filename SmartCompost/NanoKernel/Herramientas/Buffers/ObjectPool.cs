using NanoKernel.Ayudantes;
using System;
using System.Reflection;

namespace NanoKernel.Herramientas.Buffers
{
    public class ObjectPool
    {
        private static Type[] emptyType = new Type[0];
        private readonly Type objectType;
        private readonly ConcurrentQueue pool;

        public ObjectPool(Type objectType, int maxSize)
        {
            this.objectType = objectType;
            pool = new ConcurrentQueue(maxSize);

            for (int i = 0; i < maxSize; i++)
            {
                pool.Enqueue(CreateInstance());
            }
        }

        private object CreateInstance()
        {
            ConstructorInfo constructor = objectType.GetConstructor(emptyType);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {objectType.FullName} does not have a parameterless constructor.");
            }
            return constructor.Invoke(null);
        }

        public object Rent()
        {
            if (pool.Count() > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return CreateInstance(); // Si no hay objetos disponibles, crea uno nuevo
            }
        }

        public void Return(object obj)
        {
            if (obj.GetType() != objectType)
            {
                throw new ArgumentException($"Object must be of type {objectType}");
            }

            pool.Enqueue(obj);
        }
    }
}
