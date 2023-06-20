using System;
using UnityEngine;

namespace LChocolate
{
    public class LBaseObject : IDisposable
    {
        private System.Boolean disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            //Finalize();
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(System.Boolean disposing)
        {
            if (disposed)
                return;

            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~LBaseObject()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
    }
}