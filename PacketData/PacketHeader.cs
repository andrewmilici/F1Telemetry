using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace F1Telemetry.Packets
{
    public class PacketHeader
    {
        public byte[] RawData { get; set; }

        public ushort m_packetFormat { get; set; }      // 2019
        public byte m_gameMajorVersion { get; set; }     // Game major version - "X.00"
        public byte m_gameMinorVersion { get; set; }     // Game minor version - "1.XX"
        public byte m_packetVersion { get; set; }        // Version of this packet type, all start from 1
        public byte m_packetId { get; set; }             // Identifier for the packet type, see below
        public UInt64 m_sessionUID { get; set; }         // Unique identifier for the session
        public float m_sessionTime { get; set; }         // Session timestamp
        public uint m_frameIdentifier { get; set; }      // Identifier for the frame the data was retrieved on
        public byte m_playerCarIndex { get; set; }       // Index of player's car in the array

        public PacketHeader()
        {
        }

        public PacketHeader(byte[] rawData)
        {
            RawData = rawData;
            LoadData();
        }

        public long LoadData()
        {
            using (var br = new BinaryReader(new MemoryStream(RawData)))
            {
                m_packetFormat = br.ReadUInt16();
                m_gameMajorVersion = br.ReadByte();
                m_gameMinorVersion = br.ReadByte();
                m_packetVersion = br.ReadByte();
                m_packetId = br.ReadByte();
                m_sessionUID = br.ReadUInt64();
                m_sessionTime = br.ReadSingle();
                m_frameIdentifier = br.ReadUInt32();
                m_playerCarIndex = br.ReadByte();
                return br.BaseStream.Position;
            }

        }
    }
}
