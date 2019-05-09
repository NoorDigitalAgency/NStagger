﻿namespace Stagger
{
    public partial class LatinTokenizer
    {
        public const string ZZ_ATTRIBUTE_PACKED_0 =
        "\u0001\u0000\u0001\u0009\u0007\u0001\u0001\u0009\u0001\u0001\u0001\u0000\u0013\u0001\u0001\u0009\u0007\u0001\u0003\u0009" +
        "\u0001\u0001\u0004\u0000\u0001\u0009\u0003\u0000\u0001\u0001\u0002\u0000\u0001\u0001\u0001\u0000\u0003\u0001\u0001\u0000" +
        "\u0002\u0001\u0003\u0000\u0001\u0001\u0001\u0000\u0002\u0001\u0001\u0000\u0001\u0001\u0006\u0000\u0005\u0001\u0005\u0000" +
        "\u0001\u0001\u0001\u0000\u0001\u0001\u0002\u0000\u0001\u0001\u0002\u0000\u0001\u0001\u0001\u0000\u0001\u0001\u0002\u0000" +
        "\u0001\u0001\u0001\u0000\u0003\u0001\u0001\u0009\u0001\u0001";

        public const string ZZ_TRANS_PACKED_0 =
        "\u0001\u0002\u0001\u0003\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0006\u0001\u0007\u0001\u0008\u0001\u0009\u0001\u0005" +
        "\u0001\u000A\u0001\u000B\u0001\u000C\u0002\u000D\u0001\u000E\u0001\u000F\u0001\u0010\u0001\u0006\u0001\u0011\u0001\u0012" +
        "\u0001\u0013\u0001\u0014\u0001\u0015\u0001\u000A\u0001\u0008\u0001\u000A\u0001\u0005\u0001\u0007\u0001\u0016\u0001\u000A" +
        "\u0001\u0017\u0001\u0018\u0001\u0019\u0001\u001A\u0001\u001B\u0001\u001C\u0001\u000F\u0001\u001D\u0001\u0005\u0001\u000A" +
        "\u0001\u001E\u0002\u001F\u0001\u0020\u0001\u000A\u0001\u0021\u0001\u0022\u0001\u0004\u0001\u0023\u0001\u0024\u0001\u0025" +
        "\u0001\u0026\u0001\u0027\u0001\u0028\u0001\u0029\u0001\u002A\u0001\u002B\u0001\u0002\u0006\u0005\u0045\u0000\u0001\u002C" +
        "\u0005\u0000\u0001\u002C\u003B\u0000\u0004\u0004\u0002\u0000\u0002\u0004\u0007\u0000\u0003\u0004\u0001\u0000\u0001\u0004" +
        "\u0005\u0000\u0001\u0004\u0001\u0000\u0001\u0004\u0002\u0000\u0001\u0004\u0006\u0000\u0001\u0004\u0008\u0000\u0001\u0004" +
        "\u000A\u0000\u0006\u0004\u0003\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005" +
        "\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F" +
        "\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000" +
        "\u0001\u0004\u000A\u0000\u0006\u0005\u0003\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0006\u0001\u0030\u0001\u002D" +
        "\u0001\u0009\u0001\u0005\u0006\u0000\u0001\u002D\u0001\u0005\u0001\u0006\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u002E" +
        "\u0001\u0031\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0030\u0001\u0005\u0001\u0000\u0001\u0032\u0001\u0005" +
        "\u0001\u0030\u0001\u0033\u0002\u0000\u0001\u002D\u0001\u0033\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005" +
        "\u0006\u0000\u0001\u0034\u0002\u0000\u0001\u0034\u0009\u0000\u0001\u0034\u0004\u0000\u0001\u0035\u0006\u0000\u0001\u0036" +
        "\u0001\u0034\u0001\u0030\u0001\u0000\u0001\u0034\u0003\u0000\u0001\u0034\u001E\u0000\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D" +
        "\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000\u0006\u002D" +
        "\u0003\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0009\u0001\u0030\u0001\u002D\u0001\u0009\u0001\u0005\u0006\u0000" +
        "\u0001\u002D\u0001\u0005\u0001\u0009\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u0031\u0001\u0000\u0001\u002D" +
        "\u0001\u002F\u0001\u0005\u0001\u0030\u0001\u0005\u0001\u0000\u0001\u0032\u0001\u0005\u0001\u0000\u0001\u0032\u0002\u0000" +
        "\u0001\u002D\u0001\u0032\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u000C\u0000\u0001\u0037\u0001\u000C" +
        "\u0002\u000D\u001F\u0000\u0001\u0037\u0001\u000D\u001F\u0000\u0001\u000D\u003F\u0000\u0001\u000D\u0001\u0038\u0002\u0039" +
        "\u001F\u0000\u0001\u000D\u0001\u0039\u0021\u0000\u0002\u000E\u0014\u0000\u0001\u000E\u000D\u0000\u0001\u000E\u0001\u0000" +
        "\u0001\u000E\u000F\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0005\u0000\u0001\u000E\u0001\u000F" +
        "\u0003\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000" +
        "\u0001\u002D\u0004\u0000\u0001\u000F\u0001\u0000\u0001\u002D\u000B\u0000\u0001\u000E\u0001\u0000\u0001\u000E\u0005\u0000" +
        "\u0006\u002D\u0003\u0000\u0001\u0004\u0001\u0011\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005\u0006\u0000" +
        "\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005" +
        "\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004" +
        "\u000A\u0000\u0006\u0005\u0003\u0000\u0001\u0004\u0001\u003A\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005" +
        "\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F" +
        "\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000" +
        "\u0001\u0004\u000A\u0000\u0006\u0005\u0015\u0000\u0001\u0012\u002F\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0005" +
        "\u0001\u0000\u0001\u002D\u0002\u0005\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u003B\u0001\u002E\u0001\u002D" +
        "\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D" +
        "\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0001\u002D\u0001\u0014\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0002\u0000\u0002\u001F\u000F\u0000\u0006\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D" +
        "\u0006\u0000\u0004\u002D\u0001\u0000\u0002\u002D\u0001\u003C\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u003D\u0012\u0000\u0006\u002D" +
        "\u0003\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u003E\u0001\u0000\u0001\u002D\u0001\u003E\u0001\u0005\u0006\u0000" +
        "\u0001\u002D\u0001\u0005\u0001\u003E\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u003F\u0001\u0000\u0001\u002D" +
        "\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u0036\u0001\u0034\u0001\u0005\u0001\u0000\u0001\u0034\u0002\u0000" +
        "\u0001\u002D\u0001\u0034\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u0006\u0000\u0001\u0032\u0001\u0030" +
        "\u0001\u0000\u0001\u0032\u0009\u0000\u0001\u0032\u0004\u0000\u0001\u0040\u0004\u0000\u0001\u0030\u0002\u0000\u0001\u0032" +
        "\u0002\u0000\u0001\u0032\u0003\u0000\u0001\u0032\u001D\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0005\u0001\u0000" +
        "\u0001\u0041\u0002\u0005\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000" +
        "\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u0006\u0000\u0001\u0042\u000C\u0000\u0001\u0042\u000F\u0000" +
        "\u0001\u0042\u0003\u0000\u0001\u0042\u0020\u0000\u0001\u0033\u0001\u0030\u0001\u0000\u0001\u0032\u0009\u0000\u0001\u0033" +
        "\u0004\u0000\u0001\u0040\u0004\u0000\u0001\u0030\u0002\u0000\u0001\u0032\u0001\u0000\u0001\u0030\u0001\u0033\u0003\u0000" +
        "\u0001\u0033\u0022\u0000\u0001\u0043\u001B\u0000\u0001\u0044\u0035\u0000\u0001\u0045\u000C\u0000\u0001\u0046\u0001\u0047" +
        "\u0021\u0000\u0001\u0033\u0001\u0030\u0001\u0000\u0001\u0032\u0009\u0000\u0001\u0033\u0004\u0000\u0001\u0040\u0004\u0000" +
        "\u0001\u0030\u0002\u0000\u0001\u0032\u0001\u0000\u0001\u0030\u0001\u0033\u0003\u0000\u0001\u0033\u0001\u0048\u0043\u0000" +
        "\u0001\u001E\u0042\u0000\u0002\u001F\u0047\u0000\u0001\u0023\u0042\u0000\u0002\u0024\u001D\u0000\u0002\u000E\u0014\u0000" +
        "\u0001\u000E\u000C\u0000\u0001\u0024\u0001\u0025\u0001\u0000\u0001\u000E\u0040\u0000\u0002\u0026\u001B\u0000\u0002\u000E" +
        "\u0014\u0000\u0001\u000E\u000D\u0000\u0001\u000E\u0001\u0026\u0001\u0027\u0045\u0000\u0002\u002A\u000A\u0000\u0001\u0049" +
        "\u0041\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D" +
        "\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u004A\u0004\u0000\u0001\u002D" +
        "\u0001\u0000\u0001\u002D\u0013\u0000\u0001\u004B\u0001\u002D\u0001\u004C\u0003\u002D\u0004\u0000\u0001\u004D\u0001\u0000" +
        "\u0001\u004D\u0002\u0000\u0002\u004D\u0006\u0000\u0004\u004D\u0001\u0000\u0003\u004D\u0003\u0000\u0001\u004D\u0001\u0000" +
        "\u0001\u004D\u0002\u0000\u0001\u004D\u0004\u0000\u0001\u004D\u0001\u0000\u0001\u004D\u0013\u0000\u0006\u004D\u0004\u0000" +
        "\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0030\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000" +
        "\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0030\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0013\u0000\u0006\u002D\u0006\u0000\u0001\u0034\u0002\u0000\u0001\u0034\u0009\u0000\u0001\u0034\u000C\u0000" +
        "\u0001\u0034\u0002\u0000\u0001\u0034\u0003\u0000\u0001\u0034\u0020\u0000\u0001\u0034\u0002\u0000\u0001\u0034\u0009\u0000" +
        "\u0001\u0034\u0004\u0000\u0001\u0035\u0007\u0000\u0001\u0034\u0002\u0000\u0001\u0034\u0003\u0000\u0001\u0034\u0028\u0000" +
        "\u0001\u0039\u003F\u0000\u0001\u0039\u0001\u0038\u0002\u0039\u001F\u0000\u0002\u0039\u0014\u0000\u0001\u0004\u0001\u0005" +
        "\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005\u0006\u0000\u0001\u002D\u0001\u0005\u0001\u004E\u0001\u0005" +
        "\u0001\u0000\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005" +
        "\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005" +
        "\u0003\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005\u0006\u0000\u0001\u002D" +
        "\u0003\u0005\u0001\u0000\u0001\u004F\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000" +
        "\u0001\u0005\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000" +
        "\u0006\u0005\u0029\u0000\u0001\u003D\u001B\u0000\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u003E\u0001\u0000\u0001\u002D" +
        "\u0001\u003E\u0001\u0005\u0006\u0000\u0001\u002D\u0001\u0005\u0001\u003E\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u002E" +
        "\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0001\u0000\u0001\u0034\u0001\u0005" +
        "\u0001\u0000\u0001\u0034\u0002\u0000\u0001\u002D\u0001\u0034\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005" +
        "\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0050\u0001\u0000\u0001\u002D\u0001\u0050\u0001\u002D\u0006\u0000\u0002\u002D" +
        "\u0001\u0050\u0001\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0001\u0000\u0001\u0034\u0001\u002D\u0001\u0000\u0001\u0034\u0002\u0000\u0001\u002D\u0001\u0034\u0001\u002D\u0013\u0000" +
        "\u0006\u002D\u0007\u0000\u0001\u0030\u0015\u0000\u0001\u0030\u0028\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000" +
        "\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0002\u0000\u0001\u0051\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000\u0006\u002D\u0008\u0000" +
        "\u0001\u0043\u001B\u0000\u0001\u0052\u0035\u0000\u0001\u0045\u0041\u0000\u0001\u0045\u000C\u0000\u0001\u0046\u0042\u0000" +
        "\u0001\u0047\u003E\u0000\u0001\u0030\u0023\u0000\u0001\u0053\u0004\u0000\u0001\u0053\u003B\u0000\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F" +
        "\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000" +
        "\u0004\u002D\u0001\u0054\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000" +
        "\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000" +
        "\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000\u0001\u002D\u0001\u0055\u0004\u002D\u0004\u0000" +
        "\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000" +
        "\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u0056\u0004\u0000\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0013\u0000\u0006\u002D\u0004\u0000\u0001\u004D\u0001\u0000\u0001\u004D\u0002\u0000\u0002\u004D\u0006\u0000" +
        "\u0004\u004D\u0001\u0000\u0001\u004D\u0001\u0057\u0001\u004D\u0003\u0000\u0001\u004D\u0001\u0000\u0001\u004D\u0002\u0000" +
        "\u0001\u004D\u0004\u0000\u0001\u004D\u0001\u0000\u0001\u004D\u0013\u0000\u0006\u004D\u0003\u0000\u0001\u0058\u0001\u0005" +
        "\u0001\u0004\u0001\u0005\u0001\u0059\u0001\u002D\u0001\u005A\u0001\u0005\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000" +
        "\u0001\u0005\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000" +
        "\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u0003\u0000" +
        "\u0001\u0004\u0001\u0005\u0001\u0004\u0001\u0005\u0001\u0000\u0001\u002D\u0002\u0005\u0006\u0000\u0001\u002D\u0003\u0005" +
        "\u0001\u0000\u0001\u0005\u0001\u005B\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005" +
        "\u0002\u0000\u0001\u0005\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005" +
        "\u0024\u0000\u0001\u0052\u0023\u0000\u0001\u005C\u003F\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D" +
        "\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D" +
        "\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000\u0005\u002D\u0001\u005D\u0004\u0000" +
        "\u0001\u005D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000" +
        "\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000" +
        "\u0001\u002D\u0013\u0000\u0006\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u0000\u0003\u002D\u0006\u0000" +
        "\u0004\u002D\u0001\u0000\u0003\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u002D\u0001\u0000\u0001\u002D\u0002\u0000" +
        "\u0001\u002D\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u002D\u0013\u0000\u0003\u002D\u0001\u005D\u0002\u002D\u0004\u0000" +
        "\u0001\u005E\u0001\u0000\u0001\u005E\u0002\u0000\u0002\u005E\u0006\u0000\u0001\u004D\u0003\u005E\u0001\u0000\u0001\u005E" +
        "\u0001\u0057\u0001\u004D\u0003\u0000\u0001\u005E\u0001\u0000\u0001\u005E\u0002\u0000\u0001\u005E\u0004\u0000\u0001\u004D" +
        "\u0001\u0000\u0001\u005E\u0013\u0000\u0006\u005E\u0003\u0000\u0004\u0004\u0001\u0059\u0001\u0000\u0002\u0004\u0007\u0000" +
        "\u0003\u0004\u0001\u0000\u0001\u0004\u0005\u0000\u0001\u0004\u0001\u0000\u0001\u0004\u0002\u0000\u0001\u0004\u0006\u0000" +
        "\u0001\u0004\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0004\u0015\u0000\u0001\u005F\u002F\u0000\u0001\u0004\u0001\u0005" +
        "\u0001\u0004\u0001\u0005\u0001\u0059\u0001\u002D\u0002\u0005\u0006\u0000\u0001\u002D\u0003\u0005\u0001\u0000\u0001\u0005" +
        "\u0001\u002E\u0001\u002D\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0005\u0001\u0000\u0001\u0005\u0002\u0000\u0001\u0005" +
        "\u0004\u0000\u0001\u002D\u0001\u0000\u0001\u0005\u0008\u0000\u0001\u0004\u000A\u0000\u0006\u0005\u0004\u0000\u0001\u0060" +
        "\u0001\u0000\u0001\u0060\u0001\u0000\u0001\u002D\u0002\u0060\u0006\u0000\u0004\u0060\u0001\u0000\u0001\u0060\u0001\u002D" +
        "\u0001\u0060\u0001\u0000\u0001\u002D\u0001\u002F\u0001\u0060\u0001\u0000\u0001\u0060\u0002\u0000\u0001\u0060\u0004\u0000" +
        "\u0001\u0060\u0001\u0000\u0001\u0060\u0013\u0000\u0006\u0060\u0007\u0000\u0001\u0061\u003E\u0000\u0001\u0062\u0001\u0000" +
        "\u0001\u0062\u0002\u0000\u0002\u0062\u0006\u0000\u0001\u004D\u0003\u0062\u0001\u0000\u0001\u0062\u0001\u0057\u0001\u004D" +
        "\u0003\u0000\u0001\u0062\u0001\u0000\u0001\u0062\u0002\u0000\u0001\u0062\u0004\u0000\u0001\u004D\u0001\u0000\u0001\u0062" +
        "\u0013\u0000\u0006\u0062\u0015\u0000\u0001\u0063\u0030\u0000\u0001\u0060\u0001\u0000\u0001\u0060\u0001\u0064\u0001\u002D" +
        "\u0002\u0060\u0006\u0000\u0004\u0060\u0001\u0065\u0001\u0060\u0001\u005B\u0001\u0060\u0001\u0000\u0001\u002D\u0001\u002F" +
        "\u0001\u0060\u0001\u0000\u0001\u0060\u0002\u0000\u0001\u0060\u0004\u0000\u0001\u0060\u0001\u0000\u0001\u0060\u0013\u0000" +
        "\u0006\u0060\u0004\u0000\u0001\u0066\u0001\u0000\u0001\u0066\u0001\u0000\u0003\u0066\u0006\u0000\u0004\u0066\u0001\u0000" +
        "\u0001\u0066\u0001\u0000\u0001\u0066\u0003\u0000\u0001\u0066\u0001\u0000\u0001\u0066\u0002\u0000\u0001\u0066\u0004\u0000" +
        "\u0001\u0066\u0001\u0000\u0001\u0066\u0013\u0000\u0006\u0066\u0004\u0000\u0001\u0067\u0001\u0000\u0001\u0067\u0002\u0000" +
        "\u0002\u0067\u0006\u0000\u0001\u004D\u0003\u0067\u0001\u0000\u0001\u0067\u0001\u0057\u0001\u004D\u0003\u0000\u0001\u0067" +
        "\u0001\u0000\u0001\u0067\u0002\u0000\u0001\u0067\u0004\u0000\u0001\u004D\u0001\u0000\u0001\u0067\u0013\u0000\u0006\u0067" +
        "\u0004\u0000\u0001\u0068\u0001\u0000\u0001\u0068\u0002\u0000\u0002\u0068\u0006\u0000\u0004\u0068\u0001\u0000\u0001\u0068" +
        "\u0001\u0000\u0001\u0068\u0003\u0000\u0001\u0068\u0001\u0000\u0001\u0068\u0002\u0000\u0001\u0068\u0004\u0000\u0001\u0068" +
        "\u0001\u0000\u0001\u0068\u0013\u0000\u0006\u0068\u0011\u0000\u0001\u0069\u0014\u0000\u0001\u0069\u001F\u0000\u0001\u0065" +
        "\u0001\u0000\u0005\u0065\u0006\u0000\u000E\u0065\u0001\u0000\u0004\u0065\u0002\u0000\u0003\u0065\u0001\u0000\u0001\u0065" +
        "\u0001\u0000\u0001\u0065\u000F\u0000\u0006\u0065\u0004\u0000\u0001\u0066\u0001\u0000\u0001\u0066\u0001\u0000\u0003\u0066" +
        "\u0001\u006A\u0005\u0000\u0004\u0066\u0001\u0000\u0001\u0066\u0001\u0000\u0001\u0066\u0003\u0000\u0001\u0066\u0001\u0000" +
        "\u0001\u0066\u0002\u0000\u0001\u0066\u0004\u0000\u0001\u0066\u0001\u0000\u0001\u0066\u0013\u0000\u0006\u0066\u0004\u0000" +
        "\u0001\u006B\u0001\u0000\u0001\u006B\u0002\u0000\u0002\u006B\u0006\u0000\u0001\u004D\u0003\u006B\u0001\u0000\u0001\u006B" +
        "\u0001\u0057\u0001\u004D\u0003\u0000\u0001\u006B\u0001\u0000\u0001\u006B\u0002\u0000\u0001\u006B\u0004\u0000\u0001\u004D" +
        "\u0001\u0000\u0001\u006B\u0013\u0000\u0006\u006B\u0004\u0000\u0001\u0068\u0001\u0000\u0001\u0068\u0001\u0064\u0001\u0000" +
        "\u0002\u0068\u0006\u0000\u0004\u0068\u0001\u0065\u0001\u0068\u0001\u0063\u0001\u0068\u0003\u0000\u0001\u0068\u0001\u0000" +
        "\u0001\u0068\u0002\u0000\u0001\u0068\u0004\u0000\u0001\u0068\u0001\u0000\u0001\u0068\u0013\u0000\u0006\u0068\u0011\u0000" +
        "\u0001\u0069\u0003\u0000\u0001\u0065\u0010\u0000\u0001\u0069\u001C\u0000";

        public const string ZZ_ACTION_PACKED_0 =
        "\u0001\u0000\u0001\u0001\u0001\u0002\u0003\u0003\u0002\u0002\u0001\u0003\u0001\u0002\u0001\u0004\u0001\u0000\u0001\u0005" +
        "\u0002\u0006\u0002\u0003\u0001\u0002\u0001\u0003\u0001\u0007\u0001\u0008\u0001\u0003\u0001\u0002\u0001\u0003\u0006\u0002" +
        "\u0001\u0007\u0001\u0008\u0002\u0002\u0001\u0009\u0002\u000A\u0002\u000B\u0001\u000C\u0001\u000D\u0001\u000E\u0001\u0001" +
        "\u0004\u0000\u0001\u000F\u0003\u0000\u0001\u000F\u0002\u0000\u0001\u0010\u0001\u0000\u0001\u0011\u0002\u0003\u0001\u0000" +
        "\u0001\u0002\u0001\u000F\u0003\u0000\u0001\u000F\u0001\u0000\u0001\u000F\u0001\u0002\u0001\u0000\u0001\u000F\u0006\u0000" +
        "\u0002\u0003\u0003\u000F\u0005\u0000\u0001\u0003\u0001\u0000\u0001\u0003\u0002\u0000\u0001\u0012\u0002\u0000\u0001\u0012" +
        "\u0001\u0000\u0001\u0013\u0002\u0000\u0001\u0012\u0001\u0000\u0001\u0013\u0002\u0012\u0001\u0014\u0001\u0013";

        public const string ZZ_ROWMAP_PACKED_0 =
        "\u0000\u0000\u0000\u0042\u0000\u0084\u0000\u00C6\u0000\u0108\u0000\u014A\u0000\u018C\u0000\u01CE\u0000\u0210\u0000\u0042" +
        "\u0000\u0252\u0000\u0294\u0000\u02D6\u0000\u0318\u0000\u035A\u0000\u039C\u0000\u03DE\u0000\u0420\u0000\u0462\u0000\u04A4" +
        "\u0000\u04E6\u0000\u0528\u0000\u056A\u0000\u05AC\u0000\u05EE\u0000\u0630\u0000\u0672\u0000\u06B4\u0000\u06F6\u0000\u0738" +
        "\u0000\u077A\u0000\u0042\u0000\u0252\u0000\u02D6\u0000\u07BC\u0000\u07FE\u0000\u0840\u0000\u0882\u0000\u08C4\u0000\u0042" +
        "\u0000\u0042\u0000\u0042\u0000\u0906\u0000\u0948\u0000\u01CE\u0000\u098A\u0000\u09CC\u0000\u0042\u0000\u0A0E\u0000\u056A" +
        "\u0000\u0630\u0000\u0A50\u0000\u0A50\u0000\u0A92\u0000\u0252\u0000\u0AD4\u0000\u0B16\u0000\u0B58\u0000\u0B9A\u0000\u04E6" +
        "\u0000\u0BDC\u0000\u0C1E\u0000\u0C60\u0000\u0CA2\u0000\u0CE4\u0000\u05EE\u0000\u0D26\u0000\u0672\u0000\u0D68\u0000\u0DAA" +
        "\u0000\u0DEC\u0000\u0E2E\u0000\u0E70\u0000\u0EB2\u0000\u0EF4\u0000\u0F36\u0000\u0F78\u0000\u0FBA\u0000\u0FFC\u0000\u0C60" +
        "\u0000\u01CE\u0000\u103E\u0000\u1080\u0000\u10C2\u0000\u1104\u0000\u1146\u0000\u1188\u0000\u11CA\u0000\u120C\u0000\u124E" +
        "\u0000\u1290\u0000\u12D2\u0000\u01CE\u0000\u1314\u0000\u1356\u0000\u1398\u0000\u13DA\u0000\u141C\u0000\u145E\u0000\u14A0" +
        "\u0000\u14E2\u0000\u1524\u0000\u1566\u0000\u15A8\u0000\u15EA\u0000\u0042\u0000\u0F78";

        public const string ZZ_CMAP_PACKED =
        "\u0009\u0000\u0001\u000B\u0001\u000D\u0001\u0041\u0001\u0041\u0001\u000C\u0012\u0000\u0001\u000B\u0001\u002A\u0001\u002D" +
        "\u0001\u0018\u0001\u001F\u0001\u0019\u0001\u0018\u0001\u001E\u0001\u0026\u0001\u0022\u0001\u002D\u0001\u0019\u0001\u0029" +
        "\u0001\u0017\u0001\u0016\u0001\u0014\u0003\u0010\u0001\u0025\u0006\u0010\u0001\u0006\u0001\u001C\u0001\u0024\u0001\u0021" +
        "\u0001\u0028\u0001\u002B\u0001\u001A\u0002\u001B\u0001\u003D\u0001\u0005\u0001\u003C\u0001\u0013\u0001\u0040\u0001\u0011" +
        "\u0001\u0009\u0003\u001B\u0001\u003E\u0001\u003B\u0001\u0020\u0001\u0012\u0001\u001B\u0001\u003F\u0001\u0008\u0001\u0003" +
        "\u0002\u001B\u0001\u0015\u0001\u001B\u0001\u0027\u0001\u001B\u0001\u0001\u0001\u0018\u0001\u000A\u0001\u0023\u0001\u0007" +
        "\u0001\u002D\u0002\u001B\u0001\u003D\u0001\u0005\u0001\u003C\u0001\u0013\u0001\u0040\u0001\u0011\u0001\u0009\u0003\u001B" +
        "\u0001\u003E\u0001\u003B\u0001\u0020\u0001\u0012\u0001\u001B\u0001\u003F\u0001\u0008\u0001\u0003\u0002\u001B\u0001\u0015" +
        "\u0001\u001D\u0001\u0027\u0001\u001B\u0003\u002D\u0001\u0018\u0006\u0000\u0001\u000E\u001A\u0000\u0001\u000B\u001F\u002D" +
        "\u0003\u0030\u0001\u0030\u0013\u0030\u0001\u0000\u001F\u0030\u0001\u0000\u0038\u0030\u0002\u0004\u004D\u0030\u0001\u0002" +
        "\u01F0\u0000\u0090\u0031\u0200\u0000\u0060\u0032\u000A\u0033\u0086\u0032\u000A\u0033\u0006\u0032\u0050\u0000\u0030\u0032" +
        "\u0040\u0000\u000A\u000F\u0136\u0000\u0066\u0034\u000A\u0035\u0010\u0034\u0066\u0000\u000A\u000F\u0076\u0000\u000A\u000F" +
        "\u0076\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0076\u0000\u000A\u000F" +
        "\u0076\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0060\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0046\u0000\u000A\u000F" +
        "\u0116\u0000\u000A\u000F\u0046\u0000\u000A\u000F\u0066\u0000\u0100\u0037\u05E0\u0000\u000A\u000F\u0026\u0000\u000A\u000F" +
        "\u012C\u0000\u000A\u000F\u0080\u0000\u000A\u000F\u00A6\u0000\u000A\u000F\u0006\u0000\u000A\u000F\u00B6\u0000\u000A\u000F" +
        "\u0056\u0000\u000A\u000F\u0086\u0000\u000A\u000F\u0006\u0000\u000A\u000F\u0076\u0000\u0030\u0034\u0200\u0000\u0100\u0031" +
        "\u000E\u000B\u0002\u0000\u0004\u002C\u0001\u002C\u0001\u002C\u0012\u002D\u0001\u002F\u0001\u002F\u001B\u002D\u0FBB\u0000" +
        "\u0001\u002E\u001F\u002D\u0020\u0000\u00C0\u0036\u0030\u0000\u0060\u0037\u0270\u0000\u19C0\u0038\u0040\u0000\u5200\u0038" +
        "\u0620\u0000\u000A\u000F\u02A6\u0000\u000A\u000F\u0006\u0000\u0020\u0034\u000A\u000F\u0056\u0000\u0020\u0037\u0050\u0000" +
        "\u000A\u000F\u0016\u0000\u000A\u000F\u0056\u0000\u000A\u000F\u0196\u0000\u000A\u000F\u2BB6\u0000\u0050\u0037\u0040\u003A" +
        "\u0030\u0039\u0790\u003A\u1B50\u0000\u02B0\u0032\u0070\u0000\u0090\u0032\u0010\u0000\u000A\u000F\u004B\u0000\u003B\u0036" +
        "\u0500\u0000\u000A\u000F\u0BBC\u0000\u000A\u000F\u0080\u0000\u000A\u000F\u003C\u0000\u000A\u000F\u0090\u0000\u000A\u000F" +
        "\u0116\u0000\u000A\u000F\u0156\u0000\u000A\u000F\u0076\u0000\u000A\u000F\u0176\u0000\u000A\u000F\u0066\u0000\u000A\u000F" +
        "\u0066\u0000\u000A\u000F\u01A6\u0000\u000A\u000F\u0366\u0000\u000A\u000F\u4E06\u0000\u000A\u000F\u00E6\u0000\u000A\u000F" +
        "\u6C74\u0000\u0032\u000F\u1150\u0000\u000A\u000F\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000" +
        "\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\uFFFF\u0000\u16B5\u0000";
    }
}