using System;
using System.Collections.Generic;
using System.Linq;
using iText.Layout.Element;

namespace SwaggerDocCreator
{
    static class ElementExtensions
    {
        public static T SetPadding<T>(this T e, float? top, float? right, float? bottom, float? left) where T : BlockElement<T>
        {
            if (left.HasValue) e.SetPaddingLeft(left.Value);
            if (right.HasValue) e.SetPaddingRight(right.Value);
            if (top.HasValue) e.SetPaddingLeft(top.Value);
            if (bottom.HasValue) e.SetPaddingBottom(bottom.Value);
            return e;
        }

        public static T SetPadding<T>(this T e, float top, float right, float bottom, float left) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, bottom, left);
        }

        public static T SetPadding<T>(this T e, float top, float right, float bottom) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, bottom, right);
        }

        public static T SetPadding<T>(this T e, float top, float right) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, top, right);
        }

        public static T SetMargin<T>(this T e, float? top, float? right, float? bottom, float? left) where T : BlockElement<T>
        {
            if (left.HasValue) e.SetMarginLeft(left.Value);
            if (right.HasValue) e.SetMarginRight(right.Value);
            if (top.HasValue) e.SetMarginLeft(top.Value);
            if (bottom.HasValue) e.SetMarginBottom(bottom.Value);
            return e;
        }

        public static T SetMargin<T>(this T e, float top, float right, float bottom, float left) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, bottom, left);
        }

        public static T SetMargin<T>(this T e, float top, float right, float bottom) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, bottom, right);
        }

        public static T SetMargin<T>(this T e, float top, float right) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, top, right);
        }

        public static T NoMarginPadding<T>(this T e) where T : BlockElement<T>
        {
            return e.SetPadding(0).SetMargin(0);
        }
    }
}