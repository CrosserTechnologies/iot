// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SocketCan
{
    /// <summary>
    /// Set filters on the bus to read only from subset of recipients.
    /// </summary>
    public struct CanIdFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CanIdFilter"/> struct.
        /// </summary>
        /// <param name="id">Recipient Id</param>
        public CanIdFilter(uint id)
        {
            Id = id;

            if ((id & ~Interop.CAN_SFF_MASK) != 0)
            {
                Mask = (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest | Interop.CAN_EFF_MASK;
            }
            else
            {
                Mask = (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest | Interop.CAN_SFF_MASK;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanIdFilter"/> struct.
        /// </summary>
        /// <param name="id">Recipient Id</param>
        /// <param name="mask">Mask bits to use in filter</param>
        public CanIdFilter(uint id, uint mask)
        {
            Id = id;
            Mask = mask;
        }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets mask.
        /// </summary>
        public uint Mask { get; set; }
    }
}
