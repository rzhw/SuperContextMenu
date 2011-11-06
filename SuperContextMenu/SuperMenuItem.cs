/**
 * Copyright (c) 2011 Richard Z.H. Wang <http://zhwang.me/>
 * Copyright (c) 2011 wyDay <http://wyday.com/>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the copyright owners nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Zhwang.SuperContextMenu
{
    public class SuperMenuItem : MenuItem
    {
        public SuperMenuItem()
            : base()
        {
            if (!SysInfo.IsVistaOrLater)
            {
                OwnerDraw = true;
                DrawItem += PreVistaDrawItem;
                MeasureItem += PreVistaMeasureItem;
            }
        }

        public Image Image
        {
            get { return _image; }
            set { _image = value; _bitmap = (Bitmap)value; }
        }

        private Image _image;
        internal Bitmap _bitmap;
        private IntPtr _bitmapHandle = IntPtr.Zero;
        internal IntPtr BitmapHandle
        {
            get
            {
                if (_bitmapHandle == IntPtr.Zero)
                    _bitmapHandle = _bitmap.GetHbitmap();
                return _bitmapHandle;
            }
        }

        const int SEPARATOR_HEIGHT = 9;
        const int BORDER_VERTICAL = 4;
        const int LEFT_MARGIN = 4;
        const int RIGHT_MARGIN = 6;
        const int SHORTCUT_MARGIN = 20;
        const int ARROW_MARGIN = 12;
        const int ICON_SIZE = 16;

        static Font menuBoldFont = new Font(SystemFonts.MenuFont, FontStyle.Bold);

        static void PreVistaMeasureItem(object sender, MeasureItemEventArgs e)
        {
            Font font = ((MenuItem)sender).DefaultItem
                            ? menuBoldFont
                            : SystemFonts.MenuFont;

            if (((MenuItem)sender).Text == "-")
                e.ItemHeight = SEPARATOR_HEIGHT;
            else
            {
                e.ItemHeight = ((SystemFonts.MenuFont.Height > ICON_SIZE) ? SystemFonts.MenuFont.Height : ICON_SIZE)
                                + BORDER_VERTICAL;

                e.ItemWidth = LEFT_MARGIN + ICON_SIZE + RIGHT_MARGIN

                    //item text width
                    + TextRenderer.MeasureText(((MenuItem)sender).Text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoClipping).Width
                    + SHORTCUT_MARGIN

                    //shortcut text width
                    + TextRenderer.MeasureText(ShortcutToString(((MenuItem)sender).Shortcut), font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoClipping).Width

                    //arrow width
                    + ((((MenuItem)sender).IsParent) ? ARROW_MARGIN : 0);
            }
        }

        void PreVistaDrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.Low;

            bool menuSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            if (menuSelected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            else
                e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds);

            if (((MenuItem)sender).Text == "-")
            {
                //draw the separator
                int yCenter = e.Bounds.Top + (e.Bounds.Height / 2) - 1;

                e.Graphics.DrawLine(SystemPens.ControlDark, e.Bounds.Left + 1, yCenter, (e.Bounds.Left + e.Bounds.Width - 2), yCenter);
                e.Graphics.DrawLine(SystemPens.ControlLightLight, e.Bounds.Left + 1, yCenter + 1, (e.Bounds.Left + e.Bounds.Width - 2), yCenter + 1);
            }
            else //regular menu items
            {
                //draw the item text
                DrawText(sender, e, menuSelected);

                if (((MenuItem)sender).Checked)
                {
                    if (((MenuItem)sender).RadioCheck)
                    {
                        //draw the bullet
                        ControlPaint.DrawMenuGlyph(e.Graphics,
                            e.Bounds.Left + (LEFT_MARGIN + ICON_SIZE + RIGHT_MARGIN - SystemInformation.MenuCheckSize.Width) / 2,
                            e.Bounds.Top + (e.Bounds.Height - SystemInformation.MenuCheckSize.Height) / 2 + 1,
                            SystemInformation.MenuCheckSize.Width,
                            SystemInformation.MenuCheckSize.Height,
                            MenuGlyph.Bullet,
                            menuSelected ? SystemColors.HighlightText : SystemColors.MenuText,
                            menuSelected ? SystemColors.Highlight : SystemColors.Menu);
                    }
                    else
                    {
                        //draw the check mark
                        ControlPaint.DrawMenuGlyph(e.Graphics,
                            e.Bounds.Left + (LEFT_MARGIN + ICON_SIZE + RIGHT_MARGIN - SystemInformation.MenuCheckSize.Width) / 2,
                            e.Bounds.Top + (e.Bounds.Height - SystemInformation.MenuCheckSize.Height) / 2 + 1,
                            SystemInformation.MenuCheckSize.Width,
                            SystemInformation.MenuCheckSize.Height,
                            MenuGlyph.Checkmark,
                            menuSelected ? SystemColors.HighlightText : SystemColors.MenuText,
                            menuSelected ? SystemColors.Highlight : SystemColors.Menu);
                    }
                }
                else
                {
                    Image drawImg = ((SuperMenuItem)sender)._bitmap;

                    if (drawImg != null)
                    {
                        //draw the image
                        if (((MenuItem)sender).Enabled)
                            e.Graphics.DrawImage(drawImg, e.Bounds.Left + LEFT_MARGIN,
                                e.Bounds.Top + ((e.Bounds.Height - ICON_SIZE) / 2),
                                ICON_SIZE, ICON_SIZE);
                        else
                            ControlPaint.DrawImageDisabled(e.Graphics, drawImg,
                                e.Bounds.Left + LEFT_MARGIN,
                                e.Bounds.Top + ((e.Bounds.Height - ICON_SIZE) / 2),
                                SystemColors.Menu);
                    }
                }
            }
        }

        private static string ShortcutToString(Shortcut shortcut)
        {
            if (shortcut != Shortcut.None)
            {
                Keys keys = (Keys)shortcut;
                return TypeDescriptor.GetConverter(keys.GetType()).ConvertToString(keys);
            }

            return null;
        }

        private void DrawText(object sender, DrawItemEventArgs e, bool isSelected)
        {
            string shortcutText = ShortcutToString(((MenuItem)sender).Shortcut);

            int yPos = e.Bounds.Top + (e.Bounds.Height - SystemFonts.MenuFont.Height) / 2;

            Font font = ((MenuItem)sender).DefaultItem
                ? menuBoldFont
                : SystemFonts.MenuFont;

            Size textSize = TextRenderer.MeasureText(((MenuItem)sender).Text,
                                  font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);

            Rectangle textRect = new Rectangle(e.Bounds.Left + LEFT_MARGIN + ICON_SIZE + RIGHT_MARGIN, yPos,
                                   textSize.Width, textSize.Height);

            TextFormatFlags textFlags = TextFormatFlags.SingleLine
                /*| (isUsingKeyboardAccel ? 0 : TextFormatFlags.HidePrefix))*/ | TextFormatFlags.NoClipping;

            if (!((MenuItem)sender).Enabled && !isSelected) // disabled and not selected
            {
                textRect.Offset(1, 1);

                TextRenderer.DrawText(e.Graphics, ((MenuItem)sender).Text, font,
                    textRect,
                    SystemColors.ControlLightLight,
                    textFlags);

                textRect.Offset(-1, -1);
            }

            //Draw the menu item text
            TextRenderer.DrawText(e.Graphics, ((MenuItem)sender).Text, font,
                textRect,
                ((MenuItem)sender).Enabled ? (isSelected ? SystemColors.HighlightText : SystemColors.MenuText) : SystemColors.GrayText,
                textFlags);



            //Draw the shortcut text
            if (shortcutText != null)
            {
                textSize = TextRenderer.MeasureText(shortcutText,
                                  font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);


                textRect = new Rectangle(e.Bounds.Width - textSize.Width - ARROW_MARGIN, yPos, textSize.Width,
                                         textSize.Height);

                if (!((MenuItem)sender).Enabled && !isSelected) // disabled and not selected
                {
                    textRect.Offset(1, 1);

                    TextRenderer.DrawText(e.Graphics, shortcutText, font,
                        textRect,
                        SystemColors.ControlLightLight,
                        textFlags);

                    textRect.Offset(-1, -1);
                }

                TextRenderer.DrawText(e.Graphics, shortcutText, font,
                    textRect,
                    ((MenuItem)sender).Enabled ? (isSelected ? SystemColors.HighlightText : SystemColors.MenuText) : SystemColors.GrayText,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);
            }
        }
    }
}
