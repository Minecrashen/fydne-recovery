using Loli.HintsCore.Utils;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.HintsCore;

public sealed class MessageBlock
{
    readonly Dictionary<string, List<(string, float)>> replyCache = new();
    readonly Action<MessageEvent> _prepare;
    string _content;
    string _size;

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value)
                return;

            _content = value;
            replyCache.Clear();
        }
    }

    public string Size
    {
        get => _size;
        set
        {
            if (_size == value)
                return;

            _size = value;
            replyCache.Clear();
        }
    }

    public Color Color { get; set; }

    public MessageBlock(string content, Color color = default, string size = "100%", Action<MessageEvent> prepare = null)
    {
        Content = content;
        Color = color;
        Size = size;
        _prepare = prepare;
    }

    public List<(string, float)> GetContents(Player pl, DisplayBlock block, float maxSizeX, float realX)
    {
        if (_prepare is not null)
        {
            try { _prepare(new(this, pl)); }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        string cacheTag = $"{block.Align}:{maxSizeX}:{realX}";
        if (replyCache.TryGetValue(cacheTag, out var cache))
            return new(cache);

        string cont = Constants.ReplaceRegex.Replace(Content, "");

        List<(string, float)> reply = new();

        foreach (string content in cont.Split('\n'))
        {
            Vector2 blockSize = Worker.CalculateContentSize($"<size={Size}>{content}");

            if (blockSize.x < maxSizeX)
            {
                (string, float) processed = ProcessString(block, maxSizeX, realX, blockSize, content);
                reply.Add(processed);
            }
            else
            {
                List<(string, float)> processed = ProcessBigString(block, maxSizeX, realX, blockSize, content);
                reply.AddRange(processed);
            } // if-else

        } // foreach

        replyCache[cacheTag] = new(reply);

        return reply;
    }

    List<(string, float)> ProcessBigString(DisplayBlock block, float maxSizeX, float realX, Vector2 blockSize, string content)
    {
        List<(string, float)> ret = new();

        string parsed = string.Empty;
        float chX = GetCharX(block, maxSizeX, realX, blockSize);
        bool special = false;
        string cachedSpecial = string.Empty;
        string cachedUnicode = string.Empty;
        bool isRight = block.Align is Align.Right;
        List<string> sizes = new();

        if (isRight)
            RightReverse(ref content);

        IEnumerable<char> chars = content.ToCharArray();

        while (chars.Any())
        {
            char ch = chars.ElementAt(0);
            chars = chars.Skip(1);

            if (ProcessSpecial(ch, ref parsed, ref special, ref cachedSpecial, ref sizes))
                continue;

            int bytes = Encoding.UTF8.GetByteCount($"{ch}");

            if (bytes >= 3) /* rewrite and optimize later */
            {
                cachedUnicode += ch;

                if (!chars.Any())
                    continue;

                int bytesNext = Encoding.UTF8.GetByteCount($"{chars.ElementAt(0)}");
                if (bytesNext >= 3)
                    continue;
            }

            string sizeParsed = CombineSize(sizes);

            if (!string.IsNullOrEmpty(cachedUnicode))
            {
                ProcessContent(sizeParsed, isRight, cachedUnicode, ref parsed, ref chX);
                cachedUnicode = string.Empty;
            }

            if (bytes >= 3) /* rewrite and optimize later */
                continue;

            ProcessContent(sizeParsed, isRight, $"{ch}", ref parsed, ref chX);

            if (!chars.Any())
                break;

            string raw = Constants.ReplaceRegex.Replace(parsed, "");
            Vector2 rawSize = Worker.CalculateContentSize(sizeParsed + raw + chars.ElementAt(0));

            if (rawSize.x >= maxSizeX)
            {
                ret.Add((ProcessAlignContent(block, parsed), rawSize.x));

                Vector2 remainSize = Worker.CalculateContentSize($"{sizeParsed}{string.Join("", chars)}");

                parsed = string.Empty;
                chX = GetCharX(block, maxSizeX, realX, remainSize);
            }
        } // end while

        if (!string.IsNullOrEmpty(cachedUnicode))
        {
            string sizeParsed = CombineSize(sizes);
            ProcessContent(sizeParsed, isRight, cachedUnicode, ref parsed, ref chX);
        }

        if (!string.IsNullOrEmpty(parsed))
        {
            string raw = Constants.ReplaceRegex.Replace(parsed, "");
            Vector2 rawSize = Worker.CalculateContentSize($"<size={Size}>" + raw);
            ret.Add((ProcessAlignContent(block, parsed), rawSize.x));
        }

        return ret;
    }

    (string, float) ProcessString(DisplayBlock block, float maxSizeX, float realX, Vector2 blockSize, string content)
    {
        string parsed = string.Empty;
        float chX = GetCharX(block, maxSizeX, realX, blockSize);
        bool special = false;
        string cachedSpecial = string.Empty;
        string cachedUnicode = string.Empty;
        bool isRight = block.Align is Align.Right;
        List<string> sizes = new();

        if (isRight)
            RightReverse(ref content);

        if (ProcessCenterText(block, maxSizeX, realX, blockSize, out string addCenter))
        {
            parsed = addCenter + content;
            return (parsed, blockSize.x); ;
        }

        char[] chars = content.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char ch = chars[i];

            if (ProcessSpecial(ch, ref parsed, ref special, ref cachedSpecial, ref sizes))
                continue;

            int bytes = Encoding.UTF8.GetByteCount($"{ch}");

            if (bytes >= 3) /* rewrite and optimize later */
            {
                cachedUnicode += ch;

                if (i >= chars.Length - 1)
                    continue;

                int bytesNext = Encoding.UTF8.GetByteCount($"{chars[i + 1]}");
                if (bytesNext >= 3)
                    continue;
            }

            string sizeParsed = CombineSize(sizes);

            if (!string.IsNullOrEmpty(cachedUnicode))
            {
                ProcessContent(sizeParsed, isRight, cachedUnicode, ref parsed, ref chX);
                cachedUnicode = string.Empty;
            }

            if (bytes >= 3) /* rewrite and optimize later */
                continue;

            ProcessContent(sizeParsed, isRight, $"{ch}", ref parsed, ref chX);
        }

        if (!string.IsNullOrEmpty(cachedUnicode))
        {
            string sizeParsed = CombineSize(sizes);
            ProcessContent(sizeParsed, isRight, cachedUnicode, ref parsed, ref chX);
        }

        return (ProcessAlignContent(block, parsed), blockSize.x);
    }

    string ProcessAlignContent(DisplayBlock block, string content)
    {
        if (block.Align is Align.Left or Align.Right)
            return $"<align=left>{content}</align>";

        return content;
    }

    bool ProcessSpecial(char ch, ref string parsed, ref bool special, ref string cachedSpecial, ref List<string> sizes)
    {
        if (ch == '>' && special)
        {
            cachedSpecial += ch;
            special = false;

            if (cachedSpecial.StartsWith("<size="))
                sizes.Add(cachedSpecial);
            else if (cachedSpecial.StartsWith("</size>") && sizes.Count > 0)
                sizes.RemoveAt(sizes.Count - 1);

            parsed += cachedSpecial;
            cachedSpecial = string.Empty;
            return true;
        }

        if (ch == '<' || special)
        {
            cachedSpecial += ch;
            special = true;
            return true;
        }

        return false;
    }

    void ProcessContent(string sizeParsed, bool isRight, string content, ref string parsed, ref float chX)
    {
        Vector2 chSize = Worker.CalculateContentSize($"{sizeParsed}{content}");

        if (isRight)
            chX -= chSize.x;

        parsed += $"<pos={chX}>{content}";

        if (!isRight)
            chX += chSize.x;
    }

    string CombineSize(List<string> sizes)
    {
        string ret = $"<size={Size}>";

        foreach (string size in sizes)
            ret += size;

        return ret;
    }

    float GetCharX(DisplayBlock block, float maxSizeX, float realX, Vector2 blockSize)
    {
        if (block.Align is Align.Center && maxSizeX > blockSize.x)
            return -(blockSize.x / 2) + realX;

        return realX;
    }

    bool ProcessCenterText(DisplayBlock block, float maxSizeX, float realX, Vector2 blockSize, out string addStr)
    {
        addStr = string.Empty;

        if (block.Align is not Align.Center)
            return false;

        if (maxSizeX < blockSize.x)
            return false;

        if (blockSize.x + Mathf.Abs(realX) > Constants.CanvasSafeWidth)
            return false;

        if (realX == 0)
            return true;

        addStr = $"<pos={realX}>";

        return true;
    }

    void RightReverse(ref string content)
    {
        List<string> contentList = new();
        string cached = string.Empty;

        foreach (char ch in content.ToCharArray())
        {
            string chStr = ch.ToString();
            int bytes = Encoding.UTF8.GetByteCount(chStr);

            if (bytes >= 3)
            {
                cached += chStr;
                continue;
            }

            if (!string.IsNullOrEmpty(cached))
            {
                contentList.Insert(0, cached);
                cached = string.Empty;
            }

            contentList.Insert(0, chStr);
        }

        if (!string.IsNullOrEmpty(cached))
            contentList.Insert(0, cached);

        content = string.Join(string.Empty, contentList);
    } // end void

}