namespace NMSE.IO;

/// <summary>
/// Metadata for a single slot within a PS4 memory.dat file.
/// </summary>
public class MemoryDatSlot
{
    /// <summary>Gets or sets the slot index within the memory.dat file.</summary>
    public int Index { get; set; }
    /// <summary>Gets or sets whether this slot contains valid save data.</summary>
    public bool Exists { get; set; }
    /// <summary>Gets or sets the metadata format version identifier.</summary>
    public uint MetaFormat { get; set; }
    /// <summary>Gets or sets the compressed data size in bytes.</summary>
    public uint CompressedSize { get; set; }
    /// <summary>Gets or sets the byte offset of the data chunk within memory.dat.</summary>
    public uint ChunkOffset { get; set; }
    /// <summary>Gets or sets the size of the data chunk region in bytes.</summary>
    public uint ChunkSize { get; set; }
    /// <summary>Gets or sets the metadata index for this slot.</summary>
    public uint MetaIndex { get; set; }
    /// <summary>Gets or sets the save timestamp, or null if not set.</summary>
    public DateTimeOffset? Timestamp { get; set; }
    /// <summary>Gets or sets the decompressed data size in bytes.</summary>
    public uint DecompressedSize { get; set; }
    /// <summary>Gets or sets whether this slot uses the SaveWizard export format.</summary>
    public bool IsSaveWizard { get; set; }
    /// <summary>
    /// Gets or sets the absolute data offset within memory.dat for SaveWizard-format slots.
    /// This is read from field[8] (byte offset 32) of the 48-byte SaveWizard meta entry.
    /// Zero for homebrew slots (which use <see cref="ChunkOffset"/> instead).
    /// </summary>
    public uint SaveWizardDataOffset { get; set; }
}

/// <summary>
/// Reads and writes PlayStation memory.dat monolithic save files.
///
/// The memory.dat format packs all save slots (account + up to 5 saves x 2 auto/manual)
/// into a single file. Each slot has a metadata entry at a fixed offset, followed
/// by a data region where the actual (LZ4-compressed) JSON is stored.
///
/// SaveWizard-exported files have a 64-byte preamble, 48-byte per-slot meta entries,
/// and pre-decompressed JSON data. Homebrew dumps (e.g., from a modded PS4 via Save
/// Mounter or Apollo) have no preamble, 32-byte per-slot meta entries, and LZ4-compressed
/// data.
/// </summary>
public static class MemoryDatManager
{
    // Meta header value expected by the PS4 system.
    private const uint META_HEADER = 0xCA55E77E;

    // Per-slot meta entry size: 32 bytes (8 uint fields) for homebrew,
    // 48 bytes (8 standard + 4 SaveWizard extension fields) for SaveWizard.
    private const int META_LENGTH_PER_SLOT = 32;
    private const int META_LENGTH_PER_SLOT_SAVEWIZARD = 48;

    // SaveWizard magic: "NOMANSKY" in UTF-8.
    private static readonly byte[] SAVEWIZARD_HEADER = "NOMANSKY"u8.ToArray();

    // Metadata region start offsets within memory.dat.
    // Homebrew meta is at 0x00; SaveWizard meta is at 0x40 (after the 64-byte preamble).
    private const int MEMORYDAT_OFFSET_META = 0x00;             // homebrew
    private const int MEMORYDAT_OFFSET_META_SAVEWIZARD = 0x40;  // SaveWizard

    // Data region start offsets (used when writing; reads use ChunkOffset from meta).
    // Correct values differ by format.
    private const int MEMORYDAT_OFFSET_DATA_HOMEBREW = 0x20000;      // 128 KB
    private const int MEMORYDAT_OFFSET_DATA_SAVEWIZARD = 0x1040;     // 4 160 bytes after preamble
    private const uint MEMORYDAT_OFFSET_DATA_ACCOUNTDATA = 0x20000U; // fixed account-data slot
    private const uint MEMORYDAT_OFFSET_DATA_CONTAINER = 0xE0000U;   // first game-container slot

    // Per-slot data allocation sizes for homebrew.
    private const uint MEMORYDAT_LENGTH_ACCOUNTDATA = 0x40000U;  // 256 KB for account data
    private const uint MEMORYDAT_LENGTH_CONTAINER = 0x300000U;   // 3 MB per save slot

    // Total file sizes.
    // Correct homebrew size is exactly 32 MB.
    private const int MEMORYDAT_LENGTH_TOTAL = 0x2000000;            // 32 MB – homebrew
    private const int MEMORYDAT_LENGTH_TOTAL_SAVEWIZARD = 0x3000000; // 48 MB – SaveWizard

    // PS4 supports 5 game save slots, each with auto + manual = 2 containers, plus 1 account slot.
    // Correct total is 11.
    private const int MAX_SAVE_SLOTS_PS4 = 5;
    private const int MEMORYDAT_TOTAL_SLOT_COUNT = 1 + MAX_SAVE_SLOTS_PS4 * 2; // 11

    /// <summary>
    /// Check if a file is in memory.dat format (PS4 monolithic save).
    /// </summary>
    public static bool IsMemoryDat(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        var fi = new FileInfo(filePath);
        // memory.dat files are typically exactly 32MB (or close to it for SaveWizard)
        return fi.Name.Equals("memory.dat", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Detect whether a memory.dat file was created by SaveWizard.
    /// </summary>
    public static bool IsSaveWizardFormat(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        try
        {
            using var fs = File.OpenRead(filePath);
            byte[] header = new byte[SAVEWIZARD_HEADER.Length];
            if (fs.Read(header, 0, header.Length) < header.Length) return false;
            return header.AsSpan().SequenceEqual(SAVEWIZARD_HEADER);
        }
        catch { return false; }
    }

    /// <summary>
    /// Parse all save slot metadata from a memory.dat file.
    /// </summary>
    /// <param name="filePath">Path to memory.dat</param>
    /// <returns>Array of slot metadata (index 0 = account, 1-10 = save slots).</returns>
    public static MemoryDatSlot[] ReadSlots(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        bool isSaveWizard = data.Length >= SAVEWIZARD_HEADER.Length &&
            data.AsSpan(0, SAVEWIZARD_HEADER.Length).SequenceEqual(SAVEWIZARD_HEADER);

        // Homebrew meta starts at 0x00; SaveWizard meta starts at 0x40.
        int metaOffset = isSaveWizard ? MEMORYDAT_OFFSET_META_SAVEWIZARD : MEMORYDAT_OFFSET_META;
        // SaveWizard uses 48-byte meta entries; homebrew uses 32-byte entries.
        int metaPerSlot = isSaveWizard ? META_LENGTH_PER_SLOT_SAVEWIZARD : META_LENGTH_PER_SLOT;
        // PS4 supports only 5 game slots -> 11 total containers.
        int totalSlots = MEMORYDAT_TOTAL_SLOT_COUNT;
        var slots = new MemoryDatSlot[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            int slotMetaOffset = metaOffset + (i * metaPerSlot);
            if (slotMetaOffset + metaPerSlot > data.Length)
            {
                slots[i] = new MemoryDatSlot { Index = i, Exists = false };
                continue;
            }

            uint header = ReadUInt32LE(data, slotMetaOffset);
            uint format = ReadUInt32LE(data, slotMetaOffset + 4);
            uint compressedSize = ReadUInt32LE(data, slotMetaOffset + 8);
            uint chunkOffset = ReadUInt32LE(data, slotMetaOffset + 12);
            uint chunkSize = ReadUInt32LE(data, slotMetaOffset + 16);
            uint metaIndex = ReadUInt32LE(data, slotMetaOffset + 20);
            uint timestamp = ReadUInt32LE(data, slotMetaOffset + 24);
            uint decompressedSize = ReadUInt32LE(data, slotMetaOffset + 28);

            // Existence is indicated solely by a non-zero ChunkOffset. The header
            // value varies across tools (0x000007D0 for older homebrew dumps, 0xCA55E77E
            // for newer ones), so we no longer require a specific value.
            bool exists = chunkOffset != 0;

            // SaveWizard meta entries carry 16 extra bytes; field[8] at byte offset
            // 32 within the entry holds the actual pre-decompressed data offset.
            uint saveWizardDataOffset = 0;
            if (isSaveWizard)
                saveWizardDataOffset = ReadUInt32LE(data, slotMetaOffset + 32);

            slots[i] = new MemoryDatSlot
            {
                Index = i,
                Exists = exists,
                MetaFormat = format,
                CompressedSize = compressedSize,
                ChunkOffset = chunkOffset,
                ChunkSize = chunkSize,
                MetaIndex = metaIndex,
                Timestamp = timestamp > 0 ? DateTimeOffset.FromUnixTimeSeconds(timestamp) : null,
                DecompressedSize = decompressedSize,
                IsSaveWizard = isSaveWizard,
                SaveWizardDataOffset = saveWizardDataOffset,
            };
        }

        return slots;
    }

    /// <summary>
    /// Extract the JSON data for a specific slot from a memory.dat file.
    /// Returns the JSON string, or null if the slot does not exist.
    /// </summary>
    public static string? ExtractSlotData(string filePath, int slotIndex)
    {
        var slots = ReadSlots(filePath);
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        var slot = slots[slotIndex];
        if (!slot.Exists) return null;

        byte[] data = File.ReadAllBytes(filePath);

        if (slot.IsSaveWizard)
        {
            // SaveWizard pre-decompresses the JSON; the actual data offset is in
            // SaveWizardDataOffset (field[8] of the 48-byte meta entry) and the byte count
            // is DecompressedSize. No LZ4 step is needed.
            int dataOffset = (int)slot.SaveWizardDataOffset;
            int dataLength = (int)slot.DecompressedSize;

            if (dataOffset <= 0 || dataOffset + dataLength > data.Length)
                return null;

            // Trim any trailing NUL padding that SaveWizard may append.
            int lastNonNul = Array.FindLastIndex(data, dataOffset + dataLength - 1, dataLength, b => b != 0);
            int jsonEnd = lastNonNul < dataOffset ? 0 : lastNonNul - dataOffset + 1;
            return System.Text.Encoding.GetEncoding(28591).GetString(data, dataOffset, jsonEnd);
        }
        else
        {
            // Homebrew: data is LZ4-compressed at ChunkOffset with CompressedSize bytes.
            int dataOffset = (int)slot.ChunkOffset;
            int dataLength = (int)slot.CompressedSize;

            if (dataOffset <= 0 || dataOffset + dataLength > data.Length)
                return null;

            byte[] compressedData = new byte[dataLength];
            Buffer.BlockCopy(data, dataOffset, compressedData, 0, dataLength);

            byte[] decompressed = new byte[slot.DecompressedSize];
            int written = Lz4Compressor.Decompress(compressedData, 0, dataLength, decompressed, 0, (int)slot.DecompressedSize);

            return System.Text.Encoding.GetEncoding(28591).GetString(decompressed, 0, written);
        }
    }

    /// <summary>
    /// Write a complete SaveWizard memory.dat file from raw (pre-decompressed) JSON slot data.
    ///
    /// The file layout follows the SaveWizard / PS4 known specification:
    /// <list type="bullet">
    ///   <item><description>[0x0000..0x003F] 64-byte preamble: "NOMANSKY" magic bytes, meta format, meta offset, slot count, total length, then zeros.</description></item>
    ///   <item><description>[0x0040..0x03AF] 11 × 48-byte metadata entries (one per slot).</description></item>
    ///   <item><description>[0x1040..]       Pre-decompressed JSON data, packed sequentially, no LZ4.</description></item>
    ///   <item><description>Total file size: 48 MB (0x3000000).</description></item>
    /// </list>
    /// </summary>
    /// <param name="outputPath">Output file path.</param>
    /// <param name="slotData">Dictionary of slotIndex -> raw JSON bytes (Latin-1 encoded, NOT LZ4-compressed).</param>
    /// <param name="slotMeta">Dictionary of slotIndex -> slot metadata.</param>
    public static void WriteMemoryDatSaveWizard(string outputPath, Dictionary<int, byte[]> slotData, Dictionary<int, MemoryDatSlot> slotMeta)
    {
        // Allocate 48 MB for SaveWizard format.
        byte[] buffer = new byte[MEMORYDAT_LENGTH_TOTAL_SAVEWIZARD];

        using var ms = new MemoryStream(buffer);
        using var writer = new BinaryWriter(ms);

        // -- 64-byte preamble at offset 0x00 --
        // Layout:
        //   [0x00] "NOMANSKY" magic bytes (8 bytes)
        //   [0x08] meta format = 1        (4 bytes)
        //   [0x0C] meta offset = 0x40     (4 bytes)
        //   [0x10] slot count = 11        (4 bytes)
        //   [0x14] total length           (4 bytes)
        //   [0x18..0x3F] zeros            (40 bytes, already zero from buffer init)
        ms.Position = 0;
        writer.Write(SAVEWIZARD_HEADER);                         // [0x00] 8 bytes
        writer.Write((uint)1);                                   // [0x08] meta format
        writer.Write((uint)MEMORYDAT_OFFSET_META_SAVEWIZARD);    // [0x0C] meta offset = 0x40
        writer.Write((uint)MEMORYDAT_TOTAL_SLOT_COUNT);          // [0x10] slot count
        writer.Write((uint)MEMORYDAT_LENGTH_TOTAL_SAVEWIZARD);   // [0x14] total file size
        // [0x18..0x3F] left as zeros from buffer initialisation

        // -- Pre-calculate sequential data offsets --
        // Data is packed from MEMORYDAT_OFFSET_DATA_SAVEWIZARD (0x1040) with no gaps.
        var dataOffsets = new Dictionary<int, uint>();
        uint nextOffset = (uint)MEMORYDAT_OFFSET_DATA_SAVEWIZARD;
        foreach (var kvp in slotData.OrderBy(k => k.Key))
        {
            if (slotMeta.TryGetValue(kvp.Key, out var m) && m.Exists)
            {
                dataOffsets[kvp.Key] = nextOffset;
                nextOffset += (uint)kvp.Value.Length;
            }
        }

        // -- 11 × 48-byte meta entries starting at 0x40 --
        // 48-byte entry layout:
        //   +0  META_HEADER        (uint)
        //   +4  format = 1         (uint)
        //   +8  compressedSize     (uint) — equals decompressed size (no LZ4 in SW)
        //   +12 chunkOffset        (uint) — set to SAVEWIZARD_OFFSET so chunkOffset≠0 -> exists
        //   +16 chunkSize          (uint) — equals decompressed size
        //   +20 metaIndex = i      (uint)
        //   +24 timestamp          (uint)
        //   +28 decompressedSize   (uint)
        //   +32 SAVEWIZARD_OFFSET  (uint) — absolute offset of data in file (field[8])
        //   +36 padding            (uint)
        //   +40 padding            (uint)
        //   +44 padding            (uint)
        for (int i = 0; i < MEMORYDAT_TOTAL_SLOT_COUNT; i++)
        {
            ms.Position = MEMORYDAT_OFFSET_META_SAVEWIZARD + ((long)i * META_LENGTH_PER_SLOT_SAVEWIZARD);

            if (slotMeta.TryGetValue(i, out var meta) && meta.Exists
                && slotData.TryGetValue(i, out var raw)
                && dataOffsets.TryGetValue(i, out uint dataOffset))
            {
                uint jsonSize = (uint)raw.Length;
                writer.Write(META_HEADER);                                       // +0
                writer.Write((uint)1);                                           // +4
                writer.Write(jsonSize);                                          // +8  compressedSize
                writer.Write(dataOffset);                                        // +12 chunkOffset (non-zero = exists)
                writer.Write(jsonSize);                                          // +16 chunkSize
                writer.Write((uint)i);                                           // +20 metaIndex
                writer.Write((uint)(meta.Timestamp?.ToUnixTimeSeconds() ?? 0));  // +24 timestamp
                writer.Write(jsonSize);                                          // +28 decompressedSize
                writer.Write(dataOffset);                                        // +32 SAVEWIZARD_OFFSET
                writer.Write((uint)0);                                           // +36 padding
                writer.Write((uint)0);                                           // +40 padding
                writer.Write((uint)0);                                           // +44 padding
            }
            else
            {
                // Empty slot: 48 zero bytes (chunkOffset = 0 -> exists = false).
                for (int f = 0; f < META_LENGTH_PER_SLOT_SAVEWIZARD / 4; f++)
                    writer.Write((uint)0);
            }
        }

        // -- Write pre-decompressed JSON data sequentially from 0x1040 --
        foreach (var kvp in slotData.OrderBy(k => k.Key))
        {
            if (slotMeta.TryGetValue(kvp.Key, out var meta) && meta.Exists
                && dataOffsets.TryGetValue(kvp.Key, out uint dataOffset))
            {
                ms.Position = dataOffset;
                writer.Write(kvp.Value);
            }
        }

        File.WriteAllBytes(outputPath, buffer);
    }

    /// <summary>
    /// Write a complete homebrew memory.dat file from slot data.
    /// </summary>
    /// <param name="outputPath">Output file path.</param>
    /// <param name="slotData">Dictionary of slotIndex -> LZ4-compressed data bytes.</param>
    /// <param name="slotMeta">Dictionary of slotIndex -> slot metadata.</param>
    public static void WriteMemoryDat(string outputPath, Dictionary<int, byte[]> slotData, Dictionary<int, MemoryDatSlot> slotMeta)
    {
        // Allocate exactly 32 MB (0x2000000) for homebrew.
        byte[] buffer = new byte[MEMORYDAT_LENGTH_TOTAL];

        using var ms = new MemoryStream(buffer);
        using var writer = new BinaryWriter(ms);

        // Only write metadata for 11 containers (1 account + 5 slots × 2), not 31.
        for (int i = 0; i < MEMORYDAT_TOTAL_SLOT_COUNT; i++)
        {
            if (slotMeta.TryGetValue(i, out var meta) && meta.Exists)
            {
                // Write 0xCA55E77E (META_HEADER).
                writer.Write(META_HEADER);                                      // 4
                writer.Write((uint)1);                                          // 4 - format
                writer.Write(meta.CompressedSize);                              // 4
                writer.Write(meta.ChunkOffset);                                 // 4
                writer.Write(meta.ChunkSize);                                   // 4
                writer.Write((uint)i);                                          // 4 - meta index
                writer.Write((uint)(meta.Timestamp?.ToUnixTimeSeconds() ?? 0)); // 4
                writer.Write(meta.DecompressedSize);                            // 4
            }
            else
            {
                // Empty slot.
                writer.Write(META_HEADER);
                writer.Write((uint)1);
                writer.Seek(12, SeekOrigin.Current);
                writer.Write(uint.MaxValue);
                writer.Seek(8, SeekOrigin.Current);
            }
        }

        // Data region for homebrew begins at 0x20000.
        ms.Position = MEMORYDAT_OFFSET_DATA_HOMEBREW;

        foreach (var kvp in slotData.OrderBy(k => k.Key))
        {
            if (slotMeta.TryGetValue(kvp.Key, out var meta) && meta.Exists)
            {
                ms.Position = meta.ChunkOffset;
                writer.Write(kvp.Value);
            }
        }

        File.WriteAllBytes(outputPath, buffer);
    }

    private static uint ReadUInt32LE(byte[] data, int offset)
    {
        return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }
}
