using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace IronLevelDB.DB
{
    internal class VersionEdit
    {
        private readonly List<Tuple<int, InternalKey>> _compactPointers = new List<Tuple<int, InternalKey>>();
        private readonly List<Tuple<int, FileMetaData>> _newFiles = new List<Tuple<int, FileMetaData>>();

        private VersionEdit()
        {
        }

        public string Comparator { get; private set; }
        public ulong? LogNumber { get; private set; }
        public ulong? PrevLogNumber { get; private set; }
        public ulong? NextFileNumber { get; private set; }
        public ulong? LastSequence { get; private set; }

        public ISet<Tuple<int, ulong>> DeletedFiles { get; } = new HashSet<Tuple<int, ulong>>();

        public IReadOnlyCollection<Tuple<int, InternalKey>> CompactPointers => _compactPointers;

        public IReadOnlyCollection<Tuple<int, FileMetaData>> NewFiles => _newFiles;

        public static VersionEdit DecodeFrom(Stream stream)
        {
            var pb = new CodedInputStream(stream);

            var v = new VersionEdit();

            while (pb.Position < stream.Length)
            {
                var tag = (Tag) pb.ReadInt32();

                switch (tag)
                {
                    case Tag.Comparator:
                        v.Comparator = pb.ReadString();
                        break;
                    case Tag.LogNumber:
                        v.LogNumber = pb.ReadUInt64();
                        break;
                    case Tag.NextFileNumber:
                        v.NextFileNumber = pb.ReadUInt64();
                        break;
                    case Tag.LastSequence:
                        v.LastSequence = pb.ReadUInt64();
                        // TODO
                        break;
                    case Tag.CompactPointer:
                        v._compactPointers.Add(new Tuple<int, InternalKey>(
                            ReadLevel(pb), // level
                            ReadInternalKey(pb) // internal key
                        ));

                        break;
                    case Tag.DeletedFile:

                        v.DeletedFiles.Add(new Tuple<int, ulong>(
                                ReadLevel(pb), // level
                                pb.ReadUInt64()) // number
                        );

                        break;
                    case Tag.NewFile:
                        v._newFiles.Add(new Tuple<int, FileMetaData>(
                            ReadLevel(pb), // level
                            new FileMetaData
                            {
                                Number = pb.ReadUInt64(),
                                FileSize = pb.ReadUInt64(),
                                Smallest = ReadInternalKey(pb),
                                Largest = ReadInternalKey(pb)
                            })
                        );
                        break;
                    case Tag.PrevLogNumber:
                        v.PrevLogNumber = pb.ReadUInt64();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return v;
        }

        private static int ReadLevel(CodedInputStream pb)
        {
            var level = pb.ReadInt32();

            if (level > Config.NumLevels)
            {
                throw new IndexOutOfRangeException("level > " + Config.NumLevels);
            }

            return level;
        }

        private static InternalKey ReadInternalKey(CodedInputStream pb)
        {
            return new InternalKey(new ArraySegment<byte>(pb.ReadBytes().ToByteArray()));
        }

        private enum Tag
        {
            Comparator = 1,
            LogNumber = 2,
            NextFileNumber = 3,
            LastSequence = 4,
            CompactPointer = 5,
            DeletedFile = 6,
            NewFile = 7,

            // 8 was used for large value refs
            PrevLogNumber = 9
        }
    }
}