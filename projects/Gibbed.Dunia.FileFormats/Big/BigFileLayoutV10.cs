/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;

namespace Gibbed.Dunia.FileFormats.Big
{
    // Far Cry 5 / New Dawn (v10).
    //
    // Header has unknown0C/unknown10 (sub-FAT total count / sub-FAT count),
    // trailer has localization, unknown2 blocks (v7+), and sub-FATs.
    // No index encryption (that starts at v11).
    internal class BigFileLayoutV10<T> : IArchiveLayout<T>
    {
        public bool SupportsEncryption => false;

        public Stream ReadIndex(Stream input, Endian endian, bool indexIsEncrypted, int entryCount, IEntrySerializer<T> entrySerializer)
        {
            if (indexIsEncrypted == true)
            {
                throw new FormatException("encryption flag set when unsupported");
            }
            return input;
        }

        public void ReadTrailer(Stream input, Endian endian, int subFatCount, IEntrySerializer<T> entrySerializer, List<Entry<T>> entries)
        {
            // Localization section: count + per-entry (name, value).
            var localizationCount = input.ReadValueU32(endian);
            for (uint i = 0; i < localizationCount; i++)
            {
                var nameLength = input.ReadValueU32(endian);
                if (nameLength > 32)
                {
                    throw new FormatException("bad length for localization name");
                }
                input.ReadBytes((int)nameLength);
                input.ReadValueU64(endian);
            }

            // v7+: unknown2 blocks (16 bytes each), after the (empty) localization section.
            var unknown2Count = input.ReadValueU32(endian);
            for (uint i = 0; i < unknown2Count; i++)
            {
                input.Seek(16, SeekOrigin.Current);
            }

            // v9+: sub-FATs. FCBConverter merges these into the same entry set;
            // treat them as regular entries (they use the same serializer).
            if (subFatCount > 0)
            {
                for (uint i = 0; i < subFatCount; i++)
                {
                    var subFatEntries = input.ReadValueS32(endian);
                    if (subFatEntries < 0)
                    {
                        throw new FormatException();
                    }
                    for (uint j = 0; j < subFatEntries; j++)
                    {
                        entrySerializer.Deserialize(input, endian, out var entry);
                        entries.Add(entry);
                    }
                }
            }
        }
    }
}