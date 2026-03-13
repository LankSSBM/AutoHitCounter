//

using System;
using System.Collections.Generic;
using AutoHitCounter.Interfaces;
using AutoHitCounter.Models;

namespace AutoHitCounter.Services;

public class EventLogReader(IMemoryService memoryService, nint writeIndexAddr, nint bufferAddr)
{
    private int _readIndex;

    public bool IsEnabled { get; set; }

    public event Action<List<EventLogEntry>> EntriesReceived;

    public void Poll()
    {
        if (!IsEnabled) return;

        var writeIndex = memoryService.Read<int>(writeIndexAddr);
        if (writeIndex == _readIndex) return;

        var entriesToRead = (writeIndex - _readIndex) & 511;
        var dataBytes = ReadWrapping(entriesToRead);
        var entries = new List<EventLogEntry>(entriesToRead);

        for (var i = 0; i < entriesToRead; i++)
        {
            var offset = i * 5;
            var eventId = BitConverter.ToUInt32(dataBytes, offset);
            var value = dataBytes[offset + 4] != 0;
            entries.Add(new EventLogEntry(eventId, value));
        }

        _readIndex = writeIndex;
        EntriesReceived?.Invoke(entries);
    }

    private byte[] ReadWrapping(int entriesToRead)
    {
        var tail = 512 - _readIndex;
        if (entriesToRead <= tail)
            return memoryService.ReadBytes(bufferAddr + (_readIndex * 5), entriesToRead * 5);

        var part1 = memoryService.ReadBytes(bufferAddr + (_readIndex * 5), tail * 5);
        var part2 = memoryService.ReadBytes(bufferAddr, (entriesToRead - tail) * 5);
        var result = new byte[entriesToRead * 5];
        part1.CopyTo(result, 0);
        part2.CopyTo(result, part1.Length);
        return result;
    }
}
