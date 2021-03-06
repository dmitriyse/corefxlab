// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace System.Buffers.Native
{
    public unsafe sealed partial class NativeMemoryPool : MemoryPool<byte>
    {
        internal sealed class Memory : ReferenceCountedMemory<byte>
        {
            public Memory(NativeMemoryPool pool, IntPtr memory, int length)
            {
                _pool = pool;
                _pointer = memory;
                _length = length;
            }

            public IntPtr Pointer => _pointer;

            public override int Length => _length;

            public override Span<byte> Span
            {
                get
                {
                    if (IsDisposed) BuffersExperimentalThrowHelper.ThrowObjectDisposedException(nameof(NativeMemoryPool.Memory));
                    return new Span<byte>(_pointer.ToPointer(), _length);
                }
            }

            protected override void Dispose(bool disposing)
            {
                _pool.Return(this);
                base.Dispose(disposing);
            }

            protected override bool TryGetArray(out ArraySegment<byte> arraySegment)
            {
                arraySegment = default;
                return false;
            }

            public override MemoryHandle Pin(int byteOffset = 0)
            {
                Retain();
                if (byteOffset < 0 || byteOffset > _length) throw new ArgumentOutOfRangeException(nameof(byteOffset));
                return new MemoryHandle(this, Unsafe.Add<byte>(_pointer.ToPointer(), byteOffset));
            }

            private readonly NativeMemoryPool _pool;
            IntPtr _pointer;
            int _length;
        }
    }
}
