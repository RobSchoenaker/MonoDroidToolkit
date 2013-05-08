/*
 * Copyright (C) 2013 @JamesMontemagno http://www.montemagno.com http://www.refractored.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Android.App;
using Android.OS;
using Android.Views;

namespace com.refractored.monodroidtoolkit.SystemUiHider
{
    public class SystemUiHiderHoneycomb : SystemUiHiderBase
    {

#if __ANDROID_11_
        private int m_ShowFlags;
        private int m_HideFlags;
        private int m_TestFlags;
        private bool m_IsVisible = true;

        private SystemUiVisibilityChangeListner m_SystemUiVisibilityChangeListner;
#endif
        public SystemUiHiderHoneycomb(Activity activity, View anchorView, int flags)
            : base(activity, anchorView, flags)
        {
#if __ANDROID_11_
            m_ShowFlags = (int)SystemUiFlags.Visible;
            m_HideFlags = (int)SystemUiFlags.LowProfile;
            m_TestFlags = (int)SystemUiFlags.LowProfile;

            if ((m_Flags & FLAG_FULLSCREEN) != 0)
            {
                // If the client requested fullscreen, add flags relevant to hiding
                // the status bar. Note that some of these constants are new as of
                // API 16 (Jelly Bean). It is safe to use them, as they are inlined
                // at compile-time and do nothing on pre-Jelly Bean devices.
                m_ShowFlags |= (int)SystemUiFlags.LayoutFullscreen;
                m_HideFlags |= (int)SystemUiFlags.LayoutFullscreen | (int)SystemUiFlags.Fullscreen;
            }

            if ((m_Flags & FLAG_HIDE_NAVIGATION) != 0)
            {
                // If the client requested hiding navigation, add relevant flags.
                m_ShowFlags |= (int)SystemUiFlags.LayoutHideNavigation;
                m_HideFlags |= (int)SystemUiFlags.LayoutHideNavigation
                        | (int)SystemUiFlags.HideNavigation;
                m_TestFlags = (int)SystemUiFlags.HideNavigation;
            }

            m_SystemUiVisibilityChangeListner = new SystemUiVisibilityChangeListner(this);
#endif
        }



        public override void Setup()
        {
#if __ANDROID_11_
            m_AnchorView.SetOnSystemUiVisibilityChangeListener(m_SystemUiVisibilityChangeListner);
#else
            base.Setup();
#endif

        }

        public override void Show()
        {
#if __ANDROID_11_
            m_AnchorView.SystemUiVisibility = (StatusBarVisibility)m_ShowFlags;
#else
            base.Show();
#endif
        }


        public override void Hide()
        {
#if __ANDROID_11_
            m_AnchorView.SystemUiVisibility = (StatusBarVisibility)m_HideFlags;
#else
            base.Hide();
#endif
        }

#if __ANDROID_11_
        public override bool IsVisible
        {
            get
            {
                return m_IsVisible;
            }
            set
            {
                m_IsVisible = value;
            }
        }
#endif


#if __ANDROID_11_
        protected class SystemUiVisibilityChangeListner : View.IOnSystemUiVisibilityChangeListener
        {
            private SystemUiHiderHoneycomb m_SystemUiHider;

            public SystemUiVisibilityChangeListner(SystemUiHiderHoneycomb systemUiHider)
            {
                m_SystemUiHider = systemUiHider;
            }

            public void OnSystemUiVisibilityChange(StatusBarVisibility visibility)
            {
                // Test against mTestFlags to see if the system UI is visible.
                if (((int)visibility & m_SystemUiHider.m_TestFlags) != 0)
                {
                    if (Build.VERSION.SdkInt < Build.VERSION_CODES.JellyBean)
                    {
                        // Pre-Jelly Bean, we must manually hide the action bar
                        // and use the old window flags API.
                        m_SystemUiHider.m_Activity.ActionBar.Hide();
                        m_SystemUiHider.m_Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
                    }

                    // Trigger the registered listener and cache the visibility
                    // state.
                    m_SystemUiHider.m_OnVisibilityChangeListener.onVisibilityChange(false);
                    m_SystemUiHider.IsVisible = false;

                }
                else
                {
                    m_SystemUiHider.m_AnchorView.SystemUiVisibility = (StatusBarVisibility)m_SystemUiHider.m_ShowFlags;
                    if (Build.VERSION.SdkInt < Build.VERSION_CODES.JellyBean)
                    {
                        // Pre-Jelly Bean, we must manually show the action bar
                        // and use the old window flags API.
                        m_SystemUiHider.m_Activity.ActionBar.Show();
                        m_SystemUiHider.m_Activity.Window.SetFlags(0, WindowManagerFlags.Fullscreen);
                    }

                    // Trigger the registered listener and cache the visibility
                    // state.
                    m_SystemUiHider.m_OnVisibilityChangeListener.onVisibilityChange(true);
                    m_SystemUiHider.IsVisible = true;
                }
            }

            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            public IntPtr Handle
            {
                get { return m_SystemUiHider.m_Activity.Handle; }
            }
        }
#endif
    }
}