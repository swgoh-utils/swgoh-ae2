using System;
using System.Collections.Generic;
using System.Text;

namespace AssetGetterTools.models
{
    [global::ProtoBuf.ProtoContract()]
    public partial class RawAssetManifest : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"version")]
        public int Version { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"records")]
        public global::System.Collections.Generic.List<RawAssetManifestRecord> Records { get; } = new global::System.Collections.Generic.List<RawAssetManifestRecord>();

        [global::ProtoBuf.ProtoMember(3, Name = @"platform")]
        [global::System.ComponentModel.DefaultValue(null)]
        public string Platform { get; set; }

        [global::ProtoBuf.ProtoMember(4, Name = @"tex_format")]
        [global::System.ComponentModel.DefaultValue(null)]
        public string TexFormat { get; set; }

        [global::ProtoBuf.ProtoMember(5, Name = @"environment")]
        [global::System.ComponentModel.DefaultValue(null)]
        public string Environment { get; set; }

        [global::ProtoBuf.ProtoMember(6, Name = @"timestamp")]
        public ulong Timestamp { get; set; }

        [global::ProtoBuf.ProtoMember(7, Name = @"revision")]
        public ulong Revision { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RawAssetManifestRecord : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"name")]
        [global::System.ComponentModel.DefaultValue(null)]
        public string Name { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"version")]
        public ulong Version { get; set; }

        [global::ProtoBuf.ProtoMember(3, Name = @"size")]
        public int Size { get; set; }

        [global::ProtoBuf.ProtoMember(4, Name = @"uncompressed_size")]
        public int UncompressedSize { get; set; }

        [global::ProtoBuf.ProtoMember(5, Name = @"shared")]
        public bool Shared { get; set; }

        [global::ProtoBuf.ProtoMember(6, Name = @"rank")]
        public int Rank { get; set; }

        [global::ProtoBuf.ProtoMember(7)]
        public int packageType { get; set; }

        [global::ProtoBuf.ProtoMember(8, Name = @"entries")]
        public global::System.Collections.Generic.List<RawAssetManifestRecordEntry> Entries { get; } = new global::System.Collections.Generic.List<RawAssetManifestRecordEntry>();

        [global::ProtoBuf.ProtoMember(9, Name = @"dependencies")]
        public global::System.Collections.Generic.List<string> Dependencies { get; } = new global::System.Collections.Generic.List<string>();

        [global::ProtoBuf.ProtoMember(10, Name = @"crc")]
        public uint Crc { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RawAssetManifestRecordEntry : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"asset_name")]
        [global::System.ComponentModel.DefaultValue(null)]
        public string AssetName { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"runtime_size")]
        public int RuntimeSize { get; set; }

        [global::ProtoBuf.ProtoMember(3, Name = @"clone_runtime_size")]
        public int CloneRuntimeSize { get; set; }

    }
}
