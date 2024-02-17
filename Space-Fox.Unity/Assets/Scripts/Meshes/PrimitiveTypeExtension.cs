namespace SpaceFox
{
    public static class PrimitiveTypeExtension
    {
        public static bool IsPlanar(this PrimitiveType type)
            => type is PrimitiveType.Triangle or PrimitiveType.Quad;
    }
}
