/**
 * Copyright (c) 2011 Richard Z.H. Wang <http://zhwang.me/>
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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Zhwang.SuperContextMenu
{
    public class SuperContextMenu : ContextMenu
    {
        public SuperContextMenu()
            : base()
        {
            Collapse += MainCollapse;
            Popup += MainPopup;
            if (SysInfo.IsVistaOrLater)
                Popup += VistaOrLaterPopup;
        }

        public bool Visible { get; private set; }

        public void FixAfterAdd(Control control, Point point)
        {
            if (Visible && control != null)
                Show(control, point);
        }
        public void FixAfterAdd(Control control)
        {
            FixAfterAdd(control, Point.Empty);
        }
        public void FixAfterAdd()
        {
            FixAfterAdd(SourceControl);
        }

        private void MainPopup(object sender, EventArgs e)
        {
            Visible = true;
        }
        private void MainCollapse(object sender, EventArgs e)
        {
            Visible = false;
        }

        IntPtr _lastBitmapHandle = IntPtr.Zero;
        IntPtr _lastBitmapConvertedHandle = IntPtr.Zero;

        private void VistaOrLaterPopup(object sender, EventArgs e)
        {
            var menuItems = ((SuperContextMenu)sender).MenuItems;

            // For some reason, we need to manually enumerate through the items, as the index
            // otherwise is only correct when all the menu items are visible.
            int menuItemIndex = 0;
            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].Visible)
                {
                    if (menuItems[i] is SuperMenuItem)
                    {
                        SuperMenuItem menuItem = (SuperMenuItem)menuItems[i];

                        // Don't bother if we don't have an image (duh)
                        if (menuItem.Image != null)
                        {
                            // Stuff to do if we haven't converted this image before
                            if (menuItem.BitmapHandle != _lastBitmapHandle)
                            {
                                // Dispose of the old image. TODO: It's commented out as it causes issues. Figure out why.
                                //if (_lastBitmapConvertedHandle != IntPtr.Zero)
                                //    NativeMethods.DeleteObject(_lastBitmapConvertedHandle);

                                // We need to convert the image to 32bppPArgb format for use with the ContextMenu
                                using (Bitmap convertedImage = new Bitmap(menuItem._bitmap.Width, menuItem._bitmap.Height,
                                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                                {
                                    using (Graphics g = Graphics.FromImage(convertedImage))
                                        g.DrawImage(menuItem._bitmap, 0, 0, menuItem._bitmap.Width, menuItem._bitmap.Height);
                                    _lastBitmapConvertedHandle = convertedImage.GetHbitmap(Color.FromArgb(0, 0, 0, 0));
                                }
                            }

                            // Finally, set the image!
                            NativeMethods.SetMenuItemInfo(
                                new System.Runtime.InteropServices.HandleRef(null, ((Menu)sender).Handle),
                                menuItemIndex,
                                true,
                                new MENUITEMINFO_T_RW() { hbmpItem = _lastBitmapConvertedHandle });

                            // And this'll help us check if we've converted this image before
                            _lastBitmapHandle = menuItem.BitmapHandle;
                        }
                    }

                    menuItemIndex++;
                }
            }
        }
    }
}
