﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Vocaluxe.Menu;
using Vocaluxe.Menu.SingNotes;
using Vocaluxe.Menu.SongMenu;

namespace Vocaluxe.Menu
{
    struct ZSort
    {
        public int ID;
        public float z;
    }

    public abstract class CMenu : IMenu
    {        
        private List<CInteraction> _Interactions;
        private int _Selection = 0;
        private string _ThemePath = String.Empty;
        protected int _PartyModeID = -1;
                
        private List<CBackground> _Backgrounds;
        private List<CButton> _Buttons;
        private List<CText> _Texts;
        private List<CStatic> _Statics;
        private List<CSelectSlide> _SelectSlides;
        private List<CSongMenu> _SongMenus;
        private List<CLyric> _Lyrics;
        private List<CSingNotes> _SingNotes;
        private List<CNameSelection> _NameSelections;
        private List<CEqualizer> _Equalizers;
        private List<CPlaylist> _Playlists;
        private List<CParticleEffect> _ParticleEffects;

        private Hashtable _htBackgrounds;
        private Hashtable _htStatics;
        private Hashtable _htTexts;
        private Hashtable _htButtons;
        private Hashtable _htSongMenus;
        private Hashtable _htLyrics;
        private Hashtable _htSelectSlides;
        private Hashtable _htSingNotes;
        private Hashtable _htNameSelections;
        private Hashtable _htEqualizers;
        private Hashtable _htPlaylists;
        private Hashtable _htParticleEffects;


        private int _PrevMouseX;
        private int _PrevMouseY;

        protected int _MouseDX;
        protected int _MouseDY;

        protected bool _Active;

        protected int _ScreenVersion;
        protected string _ThemeName;
        protected string[] _ThemeBackgrounds;
        protected string[] _ThemeStatics;
        protected string[] _ThemeTexts;
        protected string[] _ThemeButtons;
        protected string[] _ThemeSongMenus;
        protected string[] _ThemeLyrics;
        protected string[] _ThemeSelectSlides;
        protected string[] _ThemeSingNotes;
        protected string[] _ThemeNameSelections;
        protected string[] _ThemeEqualizers;
        protected string[] _ThemePlaylists;
        protected string[] _ThemeParticleEffects;

        protected SRectF _ScreenArea;
        protected Basic _Base;

        public SRectF ScreenArea
        {
            get { return _ScreenArea; }
        }

        public SRectF GetScreenArea()
        {
            return _ScreenArea;
        }

        public int ThemeScreenVersion
        {
            get { return _ScreenVersion; }
        }

        public CMenu()
        {
        }

        public void Initialize(Basic Base)
        {
            _Base = Base;
            Init();
        }

        protected virtual void Init()
        {
            _Interactions = new List<CInteraction>();
            _Selection = 0;

            _Backgrounds = new List<CBackground>();
            _Buttons = new List<CButton>();
            _Texts = new List<CText>();
            _Statics = new List<CStatic>();
            _SelectSlides = new List<CSelectSlide>();
            _SongMenus = new List<CSongMenu>();
            _Lyrics = new List<CLyric>();
            _SingNotes = new List<CSingNotes>();
            _NameSelections = new List<CNameSelection>();
            _Equalizers = new List<CEqualizer>();
            _Playlists = new List<CPlaylist>();
            _ParticleEffects = new List<CParticleEffect>();

            _htBackgrounds = new Hashtable();
            _htStatics = new Hashtable();
            _htTexts = new Hashtable();
            _htButtons = new Hashtable();
            _htSongMenus = new Hashtable();
            _htLyrics = new Hashtable();
            _htSelectSlides = new Hashtable();
            _htSingNotes = new Hashtable();
            _htNameSelections = new Hashtable();
            _htEqualizers = new Hashtable();
            _htPlaylists = new Hashtable();
            _htParticleEffects = new Hashtable();

            _PrevMouseX = 0;
            _PrevMouseY = 0;

            _MouseDX = 0;
            _MouseDY = 0;

            _Active = false;
            _ScreenArea = new SRectF(0f, 0f, _Base.Settings.GetRenderW(), _Base.Settings.GetRenderH(), 0f);
            
            _ThemeName = String.Empty;
            _ThemeBackgrounds = null;
            _ThemeStatics = null;
            _ThemeTexts = null;
            _ThemeButtons = null;
            _ThemeSongMenus = null;
            _ThemeLyrics = null;
            _ThemeSelectSlides = null;
            _ThemeSingNotes = null;
            _ThemeNameSelections = null;
            _ThemeEqualizers = null;
            _ThemePlaylists = null;
            _ThemeParticleEffects = null;
        }

        protected void FadeTo(EScreens NextScreen)
        {
            _Base.Graphics.FadeTo(NextScreen);
        }

        #region ThemeHandler
        public virtual void LoadTheme(string XmlPath)
        {
            string file = Path.Combine(XmlPath, _ThemeName + ".xml");

            XPathDocument xPathDoc = null;
            XPathNavigator navigator = null;
            
            bool loaded = false;
            try
            {
                xPathDoc = new XPathDocument(file);
                navigator = xPathDoc.CreateNavigator();
                loaded = true;
            }
            catch (Exception e)
            {
                loaded = false;
                if (navigator != null)
                    navigator = null;

                if (xPathDoc != null)
                    xPathDoc = null;

                _Base.Log.LogError("Error loading theme file " + file + ": " + e.Message);
            }

            bool VersionCheck = false;
            if (loaded)
                VersionCheck = CheckVersion(_ScreenVersion, navigator);

            int SkinIndex = _Base.Theme.GetSkinIndex();

            if (loaded && VersionCheck && SkinIndex != -1)
            {
                _ThemePath = XmlPath;
                LoadThemeBasics(navigator, SkinIndex);

                if (_ThemeBackgrounds != null)
                {
                    for (int i = 0; i < _ThemeBackgrounds.Length; i++)
                    {
                        CBackground background = new CBackground(_Base, _PartyModeID);
                        if (background.LoadTheme("//root/" + _ThemeName, _ThemeBackgrounds[i], navigator, SkinIndex))
                        {
                            _htBackgrounds.Add(_ThemeBackgrounds[i], AddBackground(background));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Background \"" + _ThemeBackgrounds[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeStatics != null)
                {
                    for (int i = 0; i < _ThemeStatics.Length; i++)
                    {
                        CStatic stat = new CStatic(_Base, _PartyModeID);
                        if (stat.LoadTheme("//root/" + _ThemeName, _ThemeStatics[i], navigator, SkinIndex))
                        {
                            _htStatics.Add(_ThemeStatics[i], AddStatic(stat));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Static \"" + _ThemeStatics[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeTexts != null)
                {
                    for (int i = 0; i < _ThemeTexts.Length; i++)
                    {
                        CText text = new CText(_Base, _PartyModeID);
                        if (text.LoadTheme("//root/" + _ThemeName, _ThemeTexts[i], navigator, SkinIndex))
                        {
                            _htTexts.Add(_ThemeTexts[i], AddText(text));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Text \"" + _ThemeTexts[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeButtons != null)
                {
                    for (int i = 0; i < _ThemeButtons.Length; i++)
                    {
                        CButton button = new CButton(_Base, _PartyModeID);
                        if (button.LoadTheme("//root/" + _ThemeName, _ThemeButtons[i], navigator, SkinIndex))
                        {
                            _htButtons.Add(_ThemeButtons[i], AddButton(button));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Button \"" + _ThemeButtons[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeSelectSlides != null)
                {
                    for (int i = 0; i < _ThemeSelectSlides.Length; i++)
                    {
                        CSelectSlide slide = new CSelectSlide(_Base, _PartyModeID);
                        if (slide.LoadTheme("//root/" + _ThemeName, _ThemeSelectSlides[i], navigator, SkinIndex))
                        {
                            _htSelectSlides.Add(_ThemeSelectSlides[i], AddSelectSlide(slide));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load SelectSlide \"" + _ThemeSelectSlides[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeSongMenus != null)
                {
                    for (int i = 0; i < _ThemeSongMenus.Length; i++)
                    {
                        CSongMenu sm = new CSongMenu(_Base, _PartyModeID);
                        if (sm.LoadTheme("//root/" + _ThemeName, _ThemeSongMenus[i], navigator, SkinIndex))
                        {
                            _htSongMenus.Add(_ThemeSongMenus[i], AddSongMenu(sm));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load SongMenu \"" + _ThemeSongMenus[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeLyrics != null)
                {
                    for (int i = 0; i < _ThemeLyrics.Length; i++)
                    {
                        CLyric lyric = new CLyric(_Base, _PartyModeID);
                        if (lyric.LoadTheme("//root/" + _ThemeName, _ThemeLyrics[i], navigator, SkinIndex))
                        {
                            _htLyrics.Add(_ThemeLyrics[i], AddLyric(lyric));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Lyric \"" + _ThemeLyrics[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeSingNotes != null)
                {
                    for (int i = 0; i < _ThemeSingNotes.Length; i++)
                    {
                        CSingNotes notes = new CSingNotesClassic(_Base, _PartyModeID);
                        if (notes.LoadTheme("//root/" + _ThemeName, _ThemeSingNotes[i], navigator, SkinIndex))
                        {
                            _htSingNotes.Add(_ThemeSingNotes[i], AddSingNote(notes));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load SingBar \"" + _ThemeSingNotes[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeNameSelections != null)
                {
                    for (int i = 0; i < _ThemeNameSelections.Length; i++)
                    {
                        CNameSelection nsel = new CNameSelection(_Base, _PartyModeID);
                        if (nsel.LoadTheme("//root/" + _ThemeName, _ThemeNameSelections[i], navigator, SkinIndex))
                        {
                            _htNameSelections.Add(_ThemeNameSelections[i], AddNameSelection(nsel));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load NameSelection \"" + _ThemeNameSelections[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeEqualizers != null)
                {
                    for (int i = 0; i < _ThemeEqualizers.Length; i++)
                    {
                        CEqualizer eq = new CEqualizer(_Base, _PartyModeID);
                        if (eq.LoadTheme("//root/" + _ThemeName, _ThemeEqualizers[i], navigator, SkinIndex))
                        {
                            _htEqualizers.Add(_ThemeEqualizers[i], AddEqualizer(eq));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load equalizer \"" + _ThemeEqualizers[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemePlaylists != null)
                {
                    for (int i = 0; i < _ThemePlaylists.Length; i++)
                    {
                        CPlaylist pls = new CPlaylist(_Base, _PartyModeID);
                        if (pls.LoadTheme("//root/" + _ThemeName, _ThemePlaylists[i], navigator, SkinIndex))
                        {
                            _htPlaylists.Add(_ThemePlaylists[i], AddPlaylist(pls));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load Playlist \"" + _ThemePlaylists[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }

                if (_ThemeParticleEffects != null)
                {
                    for (int i = 0; i < _ThemeParticleEffects.Length; i++)
                    {
                        CParticleEffect pe = new CParticleEffect(_Base, _PartyModeID);
                        if (pe.LoadTheme("//root/" + _ThemeName, _ThemeParticleEffects[i], navigator, SkinIndex))
                        {
                            _htParticleEffects.Add(_ThemeParticleEffects[i], AddParticleEffect(pe));
                        }
                        else
                        {
                            _Base.Log.LogError("Can't load ParticleEffect \"" + _ThemeParticleEffects[i] + "\" in screen " + _ThemeName);
                        }
                    }
                }
            }
            else
            {

            }
        }

        public virtual void SaveTheme()
        {
            if (_ThemePath == String.Empty)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; 
            settings.Encoding = Encoding.UTF8;
            settings.ConformanceLevel = ConformanceLevel.Document;

            string file = Path.Combine(_ThemePath, _ThemeName + ".xml");
            XmlWriter writer = XmlWriter.Create(file, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("root");

            writer.WriteStartElement(_ThemeName);

            // Screen Version
            writer.WriteElementString("ScreenVersion", _ScreenVersion.ToString());

            // Backgrounds
            for (int i = 0; i < _Backgrounds.Count; i++)
                _Backgrounds[i].SaveTheme(writer);

            // Statics
            for (int i = 0; i < _Statics.Count; i++)
                _Statics[i].SaveTheme(writer);

            // Texts
            for (int i = 0; i < _Texts.Count; i++)
            {
                _Texts[i].SaveTheme(writer);
            }

            // Buttons
            for (int i = 0; i < _Buttons.Count; i++)
            {
                _Buttons[i].SaveTheme(writer);
            }

            // SelectSlides
            for (int i = 0; i < _SelectSlides.Count; i++)
            {
                _SelectSlides[i].SaveTheme(writer);
            }

            // SongMenus
            for (int i = 0; i < _SongMenus.Count; i++)
            {
                _SongMenus[i].SaveTheme(writer);
            }

            // Lyrics
            for (int i = 0; i < _Lyrics.Count; i++)
            {
                _Lyrics[i].SaveTheme(writer);
            }

            // SingBars
            for (int i = 0; i < _SingNotes.Count; i++)
            {
                _SingNotes[i].SaveTheme(writer);
            }

            // NameSelections
            for (int i = 0; i < _NameSelections.Count; i++)
            {
                _NameSelections[i].SaveTheme(writer);
            }

            //Equalizers
            for (int i = 0; i < _Equalizers.Count; i++)
            {
                _Equalizers[i].SaveTheme(writer);
            }

            //Playlists
            for (int i = 0; i < _Playlists.Count; i++)
            {
                _Playlists[i].SaveTheme(writer);
            }

            //ParticleEffects
            for (int i = 0; i < _ParticleEffects.Count; i++)
            {
                _ParticleEffects[i].SaveTheme(writer);
            }

            writer.WriteEndElement();

            // End of File
            writer.WriteEndElement(); //end of root
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        public virtual void ReloadTextures()
        {
            foreach (CBackground background in _Backgrounds)
            {
                background.ReloadTextures();
            }

            foreach (CButton button in _Buttons)
            {
                button.ReloadTextures();
            }

            foreach (CText text in _Texts)
            {
                text.ReloadTextures();
            }

            foreach (CStatic stat in _Statics)
            {
                stat.ReloadTextures();
            }

            foreach (CSelectSlide slide in _SelectSlides)
            {
                slide.ReloadTextures();
            }

            foreach (CSongMenu sm in _SongMenus)
            {
                sm.ReloadTextures();
            }

            foreach (CLyric lyric in _Lyrics)
            {
                lyric.ReloadTextures();
            }

            foreach (CSingNotes sn in _SingNotes)
            {
                sn.ReloadTextures();
            }

            foreach (CNameSelection ns in _NameSelections)
            {
                ns.ReloadTextures();
            }

            foreach (CEqualizer eq in _Equalizers)
            {
                eq.ReloadTextures();
            }

            foreach (CPlaylist pls in _Playlists)
            {
                pls.ReloadTextures();
            }

            foreach (CParticleEffect pe in _ParticleEffects)
            {
                pe.ReloadTextures();
            }
        }

        public virtual void UnloadTextures()
        {
            foreach (CBackground background in _Backgrounds)
            {
                background.UnloadTextures();
            }

            foreach (CButton button in _Buttons)
            {
                button.UnloadTextures();
            }

            foreach (CText text in _Texts)
            {
                text.UnloadTextures();
            }

            foreach (CStatic stat in _Statics)
            {
                stat.UnloadTextures();
            }

            foreach (CSelectSlide slide in _SelectSlides)
            {
                slide.UnloadTextures();
            }

            foreach (CSongMenu sm in _SongMenus)
            {
                sm.UnloadTextures();
            }

            foreach (CLyric lyric in _Lyrics)
            {
                lyric.UnloadTextures();
            }

            foreach (CSingNotes sn in _SingNotes)
            {
                sn.UnloadTextures();
            }

            foreach (CNameSelection ns in _NameSelections)
            {
                ns.UnloadTextures();
            }
            foreach (CPlaylist pls in _Playlists)
            {
                pls.UnloadTextures();
            }

            foreach (CEqualizer eq in _Equalizers)
            {
                eq.UnloadTextures();
            }

            foreach (CParticleEffect pe in _ParticleEffects)
            {
                pe.UnloadTextures();
            }
        }

        public virtual void ReloadTheme()
        {
            if (_ThemePath == String.Empty)
                return;

            UnloadTextures();
            Init();            
            LoadTheme(_ThemePath);
        }
        #endregion ThemeHandler

        #region GetLists
        public List<CButton> GetButtons()
        {
            return _Buttons;
        }

        public List<CText> GetTexts()
        {
            return _Texts;
        }

        public List<CBackground> GetBackgrounds()
        {
            return _Backgrounds;
        }

        public List<CStatic> GetStatics()
        {
            return _Statics;
        }

        public List<CSelectSlide> GetSelectSlides()
        {
            return _SelectSlides;
        }

        public List<CSongMenu> GetSongMenus()
        {
            return _SongMenus;
        }

        public List<CLyric> GetLyrics()
        {
            return _Lyrics;
        }

        public List<CSingNotes> GetSingNotes()
        {
            return _SingNotes;
        }

        public List<CNameSelection> GetNameSelections()
        {
            return _NameSelections;
        }

        public List<CEqualizer> GetEqualizers()
        {
            return _Equalizers;
        }

        public List<CPlaylist> GetPlaylists()
        {
            return _Playlists;
        }

        public List<CParticleEffect> GetParticleEffects()
        {
            return _ParticleEffects;
        }
        #endregion GetLists

        #region ElementHandler
        #region Create Elements
        public CButton GetNewButton()
        {
            return new CButton(_Base, _PartyModeID);
        }

        public CButton GetNewButton(CButton button)
        {
            return new CButton(button);
        }

        public CText GetNewText()
        {
            return new CText(_Base, _PartyModeID);
        }

        public CText GetNewText(CText text)
        {
            return new CText(text);
        }

        public CText GetNewText(float x, float y, float z, float h, float mw, EAlignment align, EStyle style, string font, SColorF col, string text)
        {
            return new CText(_Base, x, y, z, h, mw, align, style, font, col, text);
        }


        public CBackground GetNewBackground()
        {
            return new CBackground(_Base, _PartyModeID);
        }

        public CStatic GetNewStatic()
        {
            return new CStatic(_Base, _PartyModeID);
        }

        public CStatic GetNewStatic(CStatic Static)
        {
            return new CStatic(Static);
        }

        public CStatic GetNewStatic(STexture Texture, SColorF Color, SRectF Rect)
        {
            return new CStatic(_Base, _PartyModeID, Texture, Color, Rect);
        }

        public CSelectSlide GetNewSelectSlide()
        {
            return new CSelectSlide(_Base, _PartyModeID);
        }

        public CSelectSlide GetNewSelectSlide(CSelectSlide slide)
        {
            return new CSelectSlide(slide);
        }

        public CSongMenu GetNewSongMenu()
        {
            return new CSongMenu(_Base, _PartyModeID);
        }

        public CLyric GetNewLyric()
        {
            return new CLyric(_Base, _PartyModeID);
        }

        public CSingNotes GetNewSingNotes()
        {
            return new CSingNotesClassic(_Base, _PartyModeID);
        }

        public CNameSelection GetNewNameSelection()
        {
            return new CNameSelection(_Base, _PartyModeID);
        }

        public CEqualizer GetNewEqualizer()
        {
            return new CEqualizer(_Base, _PartyModeID);
        }

        public CPlaylist GetNewPlaylist()
        {
            return new CPlaylist(_Base, _PartyModeID);
        }

        public CParticleEffect GetNewParticleEffect(int MaxNumber, SColorF Color, SRectF Area, STexture Texture, float Size, EParticeType Type)
        {
            return new CParticleEffect(_Base, _PartyModeID, MaxNumber, Color, Area, Texture, Size, Type);
        }
        #endregion Create Elements

        #region Get Arrays
        public CButton[] Buttons
        {
            get
            {
                return _Buttons.ToArray();
            }
        }

        public CText[] Texts
        {
            get
            {
                return _Texts.ToArray();
            }
        }

        public CBackground[] Backgrounds
        {
            get
            {
                return _Backgrounds.ToArray();
            }
        }

        public CStatic[] Statics
        {
            get
            {
                return _Statics.ToArray();
            }
        }

        public CSelectSlide[] SelectSlides
        {
            get
            {
                return _SelectSlides.ToArray();
            }
        }

        public CSongMenu[] SongMenus
        {
            get
            {
                return _SongMenus.ToArray();
            }
        }

        public CLyric[] Lyrics
        {
            get
            {
                return _Lyrics.ToArray();
            }
        }

        public CSingNotes[] SingNotes
        {
            get
            {
                return _SingNotes.ToArray();
            }
        }

        public CNameSelection[] NameSelections
        {
            get
            {
                return _NameSelections.ToArray();
            }
        }

        public CEqualizer[] Equalizers
        {
            get
            {
                return _Equalizers.ToArray();
            }
        }

        public CPlaylist[] Playlists
        {
            get
            {
                return _Playlists.ToArray();
            }
        }

        public CParticleEffect[] ParticleEffects
        {
            get
            {
                return _ParticleEffects.ToArray();
            }
        }
        #endregion Get Arrays

        #region Hashtables
        public int htBackgrounds(string key)
        {
            try
            {
                return (int)_htBackgrounds[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Background Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htStatics(string key)
        {
            try
            {
                return (int)_htStatics[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Statics Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htTexts(string key)
        {
            try
            {
                return (int)_htTexts[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Text Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htButtons(string key)
        {
            try
            {
                return (int)_htButtons[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Button Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htSongMenus(string key)
        {
            try
            {
                return (int)_htSongMenus[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find SongMenu Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htLyrics(string key)
        {
            try
            {
                return (int)_htLyrics[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Lyric Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htSelectSlides(string key)
        {
            try
            {
                return (int)_htSelectSlides[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find SelectSlide Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htSingNotes(string key)
        {
            try
            {
                return (int)_htSingNotes[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find SingBar Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htNameSelections(string key)
        {
            try
            {
                return (int)_htNameSelections[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find NameSelection Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htEqualizer(string key)
        {
            try
            {
                return (int)_htEqualizers[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Equalizer Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htPlaylists(string key)
        {
            try
            {
                return (int)_htPlaylists[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find Playlist Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }

        public int htParticleEffects(string key)
        {
            try
            {
                return (int)_htParticleEffects[key];
            }
            catch (Exception)
            {
                _Base.Log.LogError("Can't find ParticleEffect Element \"" + key + "\" in Screen " + _ThemeName);
                throw;
            }
        }
        #endregion Hashtables
        #endregion ElementHandler

        #region MenuHandler
        public virtual bool HandleInput(KeyEvent KeyEvent)
        {
            if (!_Base.Settings.IsTabNavigation())
            {
                if (KeyEvent.Key == Keys.Left)
                {
                    if (_Interactions.Count > 0 && _Interactions[_Selection].Type == EType.TSelectSlide && KeyEvent.Mod != EModifier.Shift)
                        KeyEvent.Handled = PrevElement();
                    else
                        KeyEvent.Handled = _NextInteraction(KeyEvent);
                }

                if (KeyEvent.Key == Keys.Right)
                {
                    if (_Interactions.Count > 0 && _Interactions[_Selection].Type == EType.TSelectSlide && KeyEvent.Mod != EModifier.Shift)
                        KeyEvent.Handled = NextElement();
                    else
                        KeyEvent.Handled = _NextInteraction(KeyEvent);
                }

                if (KeyEvent.Key == Keys.Up || KeyEvent.Key == Keys.Down)
                {
                    KeyEvent.Handled = _NextInteraction(KeyEvent);
                }
            }
            else
            {
                if (KeyEvent.Key == Keys.Tab)
                {
                    if (KeyEvent.Mod == EModifier.Shift)
                        PrevInteraction();
                    else
                        NextInteraction();
                }

                if (KeyEvent.Key == Keys.Left)
                    PrevElement();

                if (KeyEvent.Key == Keys.Right)
                    NextElement();
            }
            if (_Base.BackgroundMusic.IsDisabled())
            {
                if (KeyEvent.ModSHIFT && (KeyEvent.Key == Keys.Add || KeyEvent.Key == Keys.PageUp))
                    _Base.Config.SetBackgroundMusicVolume(_Base.Config.GetBackgroundMusicVolume() + 5);
                else if (KeyEvent.ModSHIFT && (KeyEvent.Key == Keys.Subtract || KeyEvent.Key == Keys.PageDown))
                    _Base.Config.SetBackgroundMusicVolume(_Base.Config.GetBackgroundMusicVolume() - 5);
                else if (KeyEvent.Key == Keys.MediaNextTrack)
                    _Base.BackgroundMusic.Next();
                else if (KeyEvent.Key == Keys.MediaPreviousTrack)
                    _Base.BackgroundMusic.Previous();
                else if (KeyEvent.Key == Keys.MediaPlayPause)
                {
                    if (_Base.BackgroundMusic.IsPlaying())
                        _Base.BackgroundMusic.Pause();
                    else
                        _Base.BackgroundMusic.Play();
                }

                _Base.BackgroundMusic.ApplyVolume();
            }

            return true;
        }

        public virtual bool HandleMouse(MouseEvent MouseEvent)
        {
            int selection = _Selection;
            ProcessMouseMove(MouseEvent.X, MouseEvent.Y);
            if (selection != _Selection)
            {
                _UnsetHighlighted(selection);
                _SetHighlighted(_Selection);
            }

            if (MouseEvent.LB)
                ProcessMouseClick(MouseEvent.X, MouseEvent.Y);

            _PrevMouseX = MouseEvent.X;
            _PrevMouseY = MouseEvent.Y;

            return true;
        }

        public virtual bool HandleInputThemeEditor(KeyEvent KeyEvent)
        {
            _UnsetHighlighted(_Selection);
            if (KeyEvent.KeyPressed)
            {
                
            }
            else
            {
                switch (KeyEvent.Key)
                {
                    case Keys.S:
                        _Base.Graphics.SaveTheme();
                        break;

                    case Keys.R:
                        ReloadThemeEditMode();
                        break;

                    case Keys.Up:
                        if (KeyEvent.Mod == EModifier.Ctrl)
                            MoveElement(0, -1);

                        if (KeyEvent.Mod == EModifier.Shift)
                            ResizeElement(0, 1);

                        break;
                    case Keys.Down:
                        if (KeyEvent.Mod == EModifier.Ctrl)
                            MoveElement(0, 1);

                        if (KeyEvent.Mod == EModifier.Shift)
                            ResizeElement(0, -1);

                        break;

                    case Keys.Right:
                        if (KeyEvent.Mod == EModifier.Ctrl)
                            MoveElement(1, 0);

                        if (KeyEvent.Mod == EModifier.Shift)
                            ResizeElement(1, 0);

                        if (KeyEvent.Mod == EModifier.None)
                            NextInteraction();
                        break;
                    case Keys.Left:
                        if (KeyEvent.Mod == EModifier.Ctrl)
                            MoveElement(-1, 0);

                        if (KeyEvent.Mod == EModifier.Shift)
                            ResizeElement(-1, 0);

                        if (KeyEvent.Mod == EModifier.None)
                            PrevInteraction();
                        break;
                }
            }
            return true;
        }

        public virtual bool HandleMouseThemeEditor(MouseEvent MouseEvent)
        {
            _UnsetHighlighted(_Selection);
            _MouseDX = MouseEvent.X - _PrevMouseX;
            _MouseDY = MouseEvent.Y - _PrevMouseY;

            int stepX = 0;
            int stepY = 0;

            if ((MouseEvent.Mod & EModifier.Ctrl) == EModifier.Ctrl)
            {
                _PrevMouseX = MouseEvent.X;
                _PrevMouseY = MouseEvent.Y;
            }
            else
            {
                while (Math.Abs(MouseEvent.X - _PrevMouseX) >= 5)
                {
                    if (MouseEvent.X - _PrevMouseX >= 5)
                        stepX += 5;

                    if (MouseEvent.X - _PrevMouseX <= -5)
                        stepX -= 5;
 
                    _PrevMouseX = MouseEvent.X - (_MouseDX - stepX);
                }

                while (Math.Abs(MouseEvent.Y - _PrevMouseY) >= 5)
                {
                    if (MouseEvent.Y - _PrevMouseY >= 5)
                        stepY += 5;

                    if (MouseEvent.Y - _PrevMouseY <= -5)
                        stepY -= 5;

                    _PrevMouseY = MouseEvent.Y - (_MouseDY - stepY);
                }
            }
            
            if (MouseEvent.LBH)
            {
                //if (IsMouseOver(MouseEvent.X, _PrevMouseY))
                //{
                    if (MouseEvent.Mod == EModifier.None)
                        MoveElement(stepX, stepY);

                    if (MouseEvent.Mod == EModifier.Ctrl)
                        MoveElement(_MouseDX, _MouseDY);

                    if (MouseEvent.Mod == EModifier.Shift)
                        ResizeElement(stepX, stepY);

                    if (MouseEvent.Mod == (EModifier.Shift | EModifier.Ctrl))
                        ResizeElement(_MouseDX, _MouseDY);
                //}
            }
            else
                ProcessMouseMove(MouseEvent.X, MouseEvent.Y);

            return true;
        }

        public abstract bool UpdateGame();

        public virtual void ApplyVolume()
        {
        }

        public virtual void OnShow()
        {
            ResumeBG();
        }

        public virtual void OnShowFinish()
        {
            ResumeBG();
            _Active = true;
        }

        public virtual void OnClose()
        {
            PauseBG();
            _Active = false;
        }
        #endregion MenuHandler

        protected void ResumeBG()
        {
            foreach (CBackground bg in _Backgrounds)
            {
                bg.Resume();
            }
        }

        protected void PauseBG()
        {
            foreach (CBackground bg in _Backgrounds)
            {
                bg.Pause();
            }
        }

        #region Drawing
        public virtual bool Draw()
        {
            DrawBG();
            DrawFG();
            return true;
        }
        
        public void DrawBG()
        {
            foreach (CBackground bg in _Backgrounds)
            {
                bg.Draw();
            }
        }

        public void DrawFG()
        {
            if (_Interactions.Count <= 0)
                return;
            
            List<ZSort> items = new List<ZSort>();

            for (int i = 0; i < _Interactions.Count; i++)
            {
                if (_IsVisible(i) && (
                    _Interactions[i].Type == EType.TButton ||
                    _Interactions[i].Type == EType.TSelectSlide ||
                    _Interactions[i].Type == EType.TStatic ||
                    _Interactions[i].Type == EType.TNameSelection ||
                    _Interactions[i].Type == EType.TText ||
                    _Interactions[i].Type == EType.TSongMenu ||
                    _Interactions[i].Type == EType.TEqualizer ||
                    _Interactions[i].Type == EType.TPlaylist ||
                    _Interactions[i].Type == EType.TParticleEffect))
                {
                    ZSort zs = new ZSort();
                    zs.ID = i;
                    zs.z = _GetZValue(i);
                    items.Add(zs);
                }
            }

            if (items.Count <= 0)
                return;

                
            items.Sort(delegate(ZSort s1, ZSort s2) { return (s2.z.CompareTo(s1.z)); });

            for (int i = 0; i < items.Count; i++)
            {
                _DrawInteraction(items[i].ID);
            }
            
        }
        #endregion Drawing

        #region Elements
        public int AddBackground(CBackground bg)
        {
            _Backgrounds.Add(bg);
            _AddInteraction(_Backgrounds.Count - 1, EType.TBackground);
            return _Backgrounds.Count - 1;
        }

        public int AddButton(CButton button)
        {
            _Buttons.Add(button);
            _AddInteraction(_Buttons.Count - 1, EType.TButton);
            return _Buttons.Count - 1;
        }

        public int AddSelectSlide(CSelectSlide slide)
        {
            _SelectSlides.Add(slide);
            _AddInteraction(_SelectSlides.Count - 1, EType.TSelectSlide);
            return _SelectSlides.Count - 1;
        }

        public int AddStatic(CStatic stat)
        {
            _Statics.Add(stat);
            _AddInteraction(_Statics.Count - 1, EType.TStatic);
            return _Statics.Count - 1;
        }

        public int AddText(CText text)
        {
            _Texts.Add(text);
            _AddInteraction(_Texts.Count - 1, EType.TText);
            return _Texts.Count - 1;
        }

        public int AddSongMenu(CSongMenu songmenu)
        {
            _SongMenus.Add(songmenu);
            _AddInteraction(_SongMenus.Count - 1, EType.TSongMenu);
            return _SongMenus.Count - 1;
        }

        public int AddLyric(CLyric lyric)
        {
            _Lyrics.Add(lyric);
            _AddInteraction(_Lyrics.Count - 1, EType.TLyric);
            return _Lyrics.Count - 1;
        }

        public int AddSingNote(CSingNotes sn)
        {
            _SingNotes.Add(sn);
            _AddInteraction(_SingNotes.Count - 1, EType.TSingNote);
            return _SingNotes.Count - 1;
        }

        public int AddNameSelection(CNameSelection ns)
        {
            _NameSelections.Add(ns);
            _AddInteraction(_NameSelections.Count - 1, EType.TNameSelection);
            return _NameSelections.Count - 1;
        }

        public int AddEqualizer(CEqualizer eq)
        {
            _Equalizers.Add(eq);
            _AddInteraction(_Equalizers.Count - 1, EType.TEqualizer);
            return _Equalizers.Count - 1;
        }

        public int AddPlaylist(CPlaylist pls)
        {
            _Playlists.Add(pls);
            _AddInteraction(_Playlists.Count - 1, EType.TPlaylist);
            return _Playlists.Count - 1;
        }

        public int AddParticleEffect(CParticleEffect pe)
        {
            _ParticleEffects.Add(pe);
            _AddInteraction(_ParticleEffects.Count - 1, EType.TParticleEffect);
            return _ParticleEffects.Count - 1;
        }
        #endregion Elements

        #region InteractionHandling
        public void CheckInteraction()
        {
            if (!_IsEnabled(_Selection) || !_IsVisible(_Selection))
                PrevInteraction();
        }

        public void NextInteraction()
        {
            if (_Interactions.Count > 0)
                _NextInteraction();            
        }

        public void PrevInteraction()
        {
            if (_Interactions.Count > 0)
                _PrevInteraction();
        }

        /// <summary>
        /// Selects the next element in a menu interaction.
        /// </summary>
        /// <returns>True if the next element is selected. False if either there is no next element or the interaction does not provide such a method.</returns>
        public bool NextElement()
        {
            if (_Interactions.Count > 0)
                return _NextElement();

            return false;
        }

        /// <summary>
        /// Selects the previous element in a menu interaction.
        /// </summary>
        /// <returns>True if the previous element is selected. False if either there is no next element or the interaction does not provide such a method.</returns>
        public bool PrevElement()
        {
            if (_Interactions.Count > 0)
                return _PrevElement();

            return false;
        }

        public bool SetInteractionToButton(CButton button)
        {
            for (int i = 0; i < _Interactions.Count; i++)
            {
                if (_Interactions[i].Type == EType.TButton)
                {
                    if (_Buttons[_Interactions[i].Num] == button)
                    {
                        _UnsetSelected();
                        _UnsetHighlighted(_Selection);
                        _Selection = i;
                        _SetSelected();
                        return true;
                    }
                }
            }
            return false;
        }

        public void ProcessMouseClick(int x, int y)
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return;

            if (_Interactions[_Selection].Type == EType.TSelectSlide)
            {
                if (_SelectSlides[_Interactions[_Selection].Num].Visible)
                {
                    _SelectSlides[_Interactions[_Selection].Num].ProcessMouseLBClick(x, y);
                }
            }
        }

        public void ProcessMouseMove(int x, int y)
        {
            SelectByMouse(x, y);

            if (_Selection >= _Interactions.Count || _Selection < 0)
                return;

            if (_Interactions[_Selection].Type == EType.TSelectSlide)
            {
                if (_SelectSlides[_Interactions[_Selection].Num].Visible)
                {
                    _SelectSlides[_Interactions[_Selection].Num].ProcessMouseMove(x, y);
                }
            }
        }

        public void SelectByMouse(int x, int y)
        {
            float z = _Base.Settings.GetZFar();
            for (int i = 0; i < _Interactions.Count; i++)
            {
                if ((_Base.Settings.GetGameState() == EGameState.EditTheme) || (!_Interactions[i].ThemeEditorOnly && _IsVisible(i) && _IsEnabled(i)))
                {
                    if (_IsMouseOver(x, y, _Interactions[i]))
                    {
                        if (_GetZValue(_Interactions[i]) <= z)
                        {
                            z = _GetZValue(_Interactions[i]);
                            _UnsetSelected();
                            _Selection = i;
                            _SetSelected();
                        }
                    }
                }
            }
        }

        public bool IsMouseOver(MouseEvent MouseEvent)
        {
            return IsMouseOver(MouseEvent.X, MouseEvent.Y);
        }

        public bool IsMouseOver(int x, int y)
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return false;

            if (_Interactions.Count > 0)
                return _IsMouseOver(x, y, _Interactions[_Selection]);
            else
                return false;
        }

        private bool _IsMouseOver(int x, int y, CInteraction interact)
        {
            switch (interact.Type)
            {
                case EType.TButton:
                    if (CHelper.IsInBounds(_Buttons[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TSelectSlide:
                    if (CHelper.IsInBounds(_SelectSlides[interact.Num].Rect, x, y) ||
                        CHelper.IsInBounds(_SelectSlides[interact.Num].RectArrowLeft, x, y) ||
                        CHelper.IsInBounds(_SelectSlides[interact.Num].RectArrowRight, x, y))
                        return true;
                    break;
                case EType.TStatic:
                    if (CHelper.IsInBounds(_Statics[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TText:
                    RectangleF bounds = _Base.Drawing.GetTextBounds(_Texts[interact.Num]);
                    if (CHelper.IsInBounds(new SRectF(_Texts[interact.Num].X, _Texts[interact.Num].Y, bounds.Width, bounds.Height, _Texts[interact.Num].Z), x, y))
                        return true;
                    break;
                case EType.TSongMenu:
                    if (CHelper.IsInBounds(_SongMenus[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TLyric:
                    if (CHelper.IsInBounds(_Lyrics[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TNameSelection:
                    if (CHelper.IsInBounds(_NameSelections[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TEqualizer:
                    if (CHelper.IsInBounds(_Equalizers[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TPlaylist:
                    if (CHelper.IsInBounds(_Playlists[interact.Num].Rect, x, y))
                        return true;
                    break;
                case EType.TParticleEffect:
                    if (CHelper.IsInBounds(_ParticleEffects[interact.Num].Rect, x, y))
                        return true;
                    break;
            } 
            return false;
        }

        private float _GetZValue(CInteraction interact)
        {
            switch (interact.Type)
            {
                case EType.TButton:
                    return _Buttons[interact.Num].Rect.Z;
                case EType.TSelectSlide:
                    return _SelectSlides[interact.Num].Rect.Z;
                case EType.TStatic:
                    return _Statics[interact.Num].Rect.Z;
                case EType.TSongMenu:
                    return _SongMenus[interact.Num].Rect.Z;
                case EType.TText:
                    return _Texts[interact.Num].Z;
                case EType.TLyric:
                    return _Lyrics[interact.Num].Rect.Z;
                case EType.TNameSelection:
                    return _NameSelections[interact.Num].Rect.Z;
                case EType.TEqualizer:
                    return _Equalizers[interact.Num].Rect.Z;
                case EType.TPlaylist:
                    return _Playlists[interact.Num].Rect.Z;
                case EType.TParticleEffect:
                    return _ParticleEffects[interact.Num].Rect.Z;
            }
            return _Base.Settings.GetZFar();
        }

        private float _GetZValue(int interaction)
        {
            switch (_Interactions[interaction].Type)
            {
                case EType.TButton:
                    return _Buttons[_Interactions[interaction].Num].Rect.Z;

                case EType.TSelectSlide:
                    return _SelectSlides[_Interactions[interaction].Num].Rect.Z;

                case EType.TStatic:
                    return _Statics[_Interactions[interaction].Num].Rect.Z;

                case EType.TText:
                    return _Texts[_Interactions[interaction].Num].Z;

                case EType.TSongMenu:
                    return _SongMenus[_Interactions[interaction].Num].Rect.Z;

                case EType.TLyric:
                    return _Lyrics[_Interactions[interaction].Num].Rect.Z;

                case EType.TNameSelection:
                    return _NameSelections[_Interactions[interaction].Num].Rect.Z;

                case EType.TEqualizer:
                    return _Equalizers[_Interactions[interaction].Num].Rect.Z;

                case EType.TPlaylist:
                    return _Playlists[_Interactions[interaction].Num].Rect.Z;

                case EType.TParticleEffect:
                    return _ParticleEffects[_Interactions[interaction].Num].Rect.Z;
            }

            return _Base.Settings.GetZFar();
        }

        private void _NextInteraction()
        {
            _UnsetSelected();
            if (_Base.Settings.GetGameState() != EGameState.EditTheme)
            {
                bool found = false;
                int start = _Selection;
                do
                {
                    start++;
                    if (start > _Interactions.Count - 1)
                        start = 0;

                    if ((start == _Selection) || (!_Interactions[start].ThemeEditorOnly && _IsVisible(start) && _IsEnabled(start)))
                        found = true;
                } while (!found);
                _Selection = start;
            }
            else
            {
                _Selection++;
                if (_Selection > _Interactions.Count - 1)
                    _Selection = 0;
            }
            _SetSelected();
        }

        private void _PrevInteraction()
        {
            _UnsetSelected();
            if (_Base.Settings.GetGameState() != EGameState.EditTheme)
            {
                bool found = false;
                int start = _Selection;
                do
                {
                    start--;
                    if (start < 0)
                        start = _Interactions.Count - 1;

                    if ((start == _Selection) || (!_Interactions[start].ThemeEditorOnly && _IsVisible(start) && _IsEnabled(start)))
                        found = true;
                } while (!found);
                _Selection = start;
            }
            else
            {
                _Selection--;
                if (_Selection < 0)
                    _Selection = _Interactions.Count - 1;
            }
            _SetSelected();
        }

        /// <summary>
        /// Selects the next best interaction in a menu.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        private bool _NextInteraction(KeyEvent Key)
        {
            KeyEvent[] Directions = new KeyEvent[4];
            float[] Distances = new float[4];
            int[] stages = new int[4];
            int[] elements = new int[4];

            for (int i = 0; i < 4; i++)
            {
                Directions[i] = new KeyEvent();
            }

            Directions[0].Key = Keys.Up;
            Directions[1].Key = Keys.Right;
            Directions[2].Key = Keys.Down;
            Directions[3].Key = Keys.Left;

            for (int i = 0; i < 4; i++)
            {
                elements[i] = _GetNextElement(Directions[i], out Distances[i], out stages[i]);
            }

            int element = _Selection;
            int stage = int.MaxValue;
            float distance = float.MaxValue;
            int direction = -1;

            int mute = -1;
            switch (Key.Key)
            {
                case Keys.Up:
                    mute = 2;
                    break;
                case Keys.Right:
                    mute = 3;
                    break;
                case Keys.Down:
                    mute = 0;
                    break;
                case Keys.Left:
                    mute = 1;
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (i != mute && elements[i] != _Selection && (stages[i] < stage || (stages[i] == stage && Directions[i].Key == Key.Key)))
                {
                    stage = stages[i];
                    element = elements[i];
                    distance = Distances[i];
                    direction = i;
                }
            }

            if (direction != -1)
            {
                // select the new element
                if (Directions[direction].Key == Key.Key)
                {
                    _UnsetHighlighted(_Selection);
                    _UnsetSelected();

                    _Selection = element;
                    _SetSelected();
                    _SetHighlighted(_Selection);

                    return true;
                }
            }
            return false;
        }

        private int _GetNextElement(KeyEvent Key, out float Distance, out int Stage)
        {
            Distance = float.MaxValue;
            int min = _Selection;
            SRectF actualRect = _GetRect(_Selection);
            Stage = int.MaxValue;

            for (int i = 0; i < _Interactions.Count; i++)
            {
                if (i != _Selection && !_Interactions[i].ThemeEditorOnly && _IsVisible(i) && _IsEnabled(i))
                {
                    SRectF targetRect = _GetRect(i);
                    float dist = _GetDistanceDirect(Key, actualRect, targetRect);
                    if (dist >= 0f && dist < Distance)
                    {
                        Distance = dist;
                        min = i;
                        Stage = 10;
                    }
                }
            }

            if (min == _Selection)
            {
                for (int i = 0; i < _Interactions.Count; i++)
                {
                    if (i != _Selection && !_Interactions[i].ThemeEditorOnly && _IsVisible(i) && _IsEnabled(i))
                    {
                        SRectF targetRect = _GetRect(i);
                        float dist = _GetDistance180(Key, actualRect, targetRect);
                        if (dist >= 0f && dist < Distance)
                        {
                            Distance = dist;
                            min = i;
                            Stage = 20;
                        }
                    }
                }
            }

            if (min == _Selection)
            {
                switch (Key.Key)
                {
                    case Keys.Up:
                        actualRect = new SRectF(actualRect.X, _Base.Settings.GetRenderH(), 1, 1, actualRect.Z);
                        break;
                    case Keys.Down:
                        actualRect = new SRectF(actualRect.X, 0, 1, 1, actualRect.Z);
                        break;
                    case Keys.Left:
                        actualRect = new SRectF(_Base.Settings.GetRenderW(), actualRect.Y, 1, 1, actualRect.Z);
                        break;
                    case Keys.Right:
                        actualRect = new SRectF(0, actualRect.Y, 1, 1, actualRect.Z);
                        break;
                    default:
                        break;
                }

                for (int i = 0; i < _Interactions.Count; i++)
                {
                    if (i != _Selection && !_Interactions[i].ThemeEditorOnly && _IsVisible(i) && _IsEnabled(i))
                    {
                        SRectF targetRect = _GetRect(i);
                        float dist = _GetDistance180(Key, actualRect, targetRect);
                        if (dist >= 0f && dist < Distance)
                        {
                            Distance = dist;
                            min = i;
                            Stage = 30;
                        }
                    }
                }
            }
            return min;
        }

        private float _GetDistanceDirect(KeyEvent Key, SRectF actualRect, SRectF targetRect)
        {
            PointF source = new PointF(actualRect.X + actualRect.W / 2f, actualRect.Y + actualRect.H / 2f);
            PointF dest = new PointF(targetRect.X + targetRect.W / 2f, targetRect.Y + targetRect.H / 2f);

            PointF vector = new PointF(dest.X - source.X, dest.Y - source.Y);
            float distance = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            bool inDirection = false;
            switch (Key.Key)
            {
                case Keys.Up:
                    if (vector.Y < 0f && (targetRect.X + targetRect.W > actualRect.X && actualRect.X + actualRect.W > targetRect.X))
                        inDirection = true;
                    break;

                case Keys.Down:
                    if (vector.Y > 0f && (targetRect.X + targetRect.W > actualRect.X && actualRect.X + actualRect.W > targetRect.X))
                        inDirection = true;
                    break;

                case Keys.Left:
                    if (vector.X < 0f && (targetRect.Y + targetRect.H > actualRect.Y && actualRect.Y + actualRect.H > targetRect.Y))
                        inDirection = true;
                    break;

                case Keys.Right:
                    if (vector.X > 0f && (targetRect.Y + targetRect.H > actualRect.Y && actualRect.Y + actualRect.H > targetRect.Y))
                        inDirection = true;
                    break;

                default:
                    break;
            }
            if (!inDirection)
                return float.MaxValue;
            else
                return distance;
        }

        private float _GetDistance180(KeyEvent Key, SRectF actualRect, SRectF targetRect)
        {
            PointF source = new PointF(actualRect.X + actualRect.W / 2f, actualRect.Y + actualRect.H / 2f);
            PointF dest = new PointF(targetRect.X + targetRect.W / 2f, targetRect.Y + targetRect.H / 2f);

            PointF vector = new PointF(dest.X - source.X, dest.Y - source.Y);
            float distance = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            bool inDirection = false;
            switch (Key.Key)
            {
                case Keys.Up:
                    if (vector.Y < 0f)
                        inDirection = true;
                    break;

                case Keys.Down:
                    if (vector.Y > 0f)
                        inDirection = true;
                    break;

                case Keys.Left:
                    if (vector.X < 0f)
                        inDirection = true;
                    break;

                case Keys.Right:
                    if (vector.X > 0f)
                        inDirection = true;
                    break;

                default:
                    break;
            }
            if (!inDirection)
                return float.MaxValue;
            else
                return distance;
        }

        /// <summary>
        /// Selects the next element in a menu interaction.
        /// </summary>
        /// <returns>True if the next element is selected. False if either there is no next element or the interaction does not provide such a method.</returns>
        private bool _NextElement()
        {
            if (_Interactions[_Selection].Type == EType.TSelectSlide)
                return _SelectSlides[_Interactions[_Selection].Num].NextValue();

            return false;
        }

        /// <summary>
        /// Selects the previous element in a menu interaction.
        /// </summary>
        /// <returns>
        /// True if the previous element is selected. False if either there is no previous element or the interaction does not provide such a method.
        /// </returns>
        private bool _PrevElement()
        {
            if (_Interactions[_Selection].Type == EType.TSelectSlide)
                return _SelectSlides[_Interactions[_Selection].Num].PrevValue();

            return false;
        }

        private void _SetSelected()
        {
            switch (_Interactions[_Selection].Type)
            {
                case EType.TButton:
                    _Buttons[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TSelectSlide:
                    _SelectSlides[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TStatic:
                    _Statics[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TText:
                    _Texts[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TSongMenu:
                    _SongMenus[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TLyric:
                    _Lyrics[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TNameSelection:
                    _NameSelections[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TEqualizer:
                    _Equalizers[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TPlaylist:
                    _Playlists[_Interactions[_Selection].Num].Selected = true;
                    break;
                case EType.TParticleEffect:
                    _ParticleEffects[_Interactions[_Selection].Num].Selected = true;
                    break;
            }
        }

        private void _UnsetSelected()
        {
            switch (_Interactions[_Selection].Type)
            {
                case EType.TButton:
                    _Buttons[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TSelectSlide:
                    _SelectSlides[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TStatic:
                    _Statics[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TText:
                    _Texts[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TSongMenu:
                    _SongMenus[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TLyric:
                    _Lyrics[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TNameSelection:
                    _NameSelections[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TEqualizer:
                    _Equalizers[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TPlaylist:
                    _Playlists[_Interactions[_Selection].Num].Selected = false;
                    break;
                case EType.TParticleEffect:
                    _ParticleEffects[_Interactions[_Selection].Num].Selected = false;
                    break;
            }
        }

        private void _SetHighlighted(int selection)
        {
            if (selection >= _Interactions.Count || selection < 0)
                return;

            switch (_Interactions[selection].Type)
            {
                case EType.TButton:
                    //_Buttons[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TSelectSlide:
                    _SelectSlides[_Interactions[selection].Num].Highlighted = true;
                    break;
                case EType.TStatic:
                    //_Statics[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TText:
                    //_Texts[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TSongMenu:
                    //_SongMenus[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TLyric:
                    //_Lyrics[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TNameSelection:
                    //_NameSelections[_Interactions[selection].Num].Selected = true;
                    break;
                case EType.TPlaylist:
                    //_Playlists[_Interactions[_Selection].Num].Selected = true;
                    break;
            }
        }

        private void _UnsetHighlighted(int selection)
        {
            if (selection >= _Interactions.Count || selection < 0)
                return;

            switch (_Interactions[selection].Type)
            {
                case EType.TButton:
                    //_Buttons[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TSelectSlide:
                    _SelectSlides[_Interactions[selection].Num].Highlighted = false;
                    break;
                case EType.TStatic:
                    //_Statics[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TText:
                    //_Texts[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TSongMenu:
                    //_SongMenus[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TLyric:
                    //_Lyrics[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TNameSelection:
                    //_NameSelections[_Interactions[selection].Num].Selected = false;
                    break;
                case EType.TPlaylist:
                    //_Playlists[_Interactions[_Selection].Num].Selected = true;
                    break;
            }
        }

        private void _ToggleHighlighted()
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return;

            if (_Interactions[_Selection].Type == EType.TSelectSlide)
            {
                _SelectSlides[_Interactions[_Selection].Num].Highlighted = !_SelectSlides[_Interactions[_Selection].Num].Highlighted;
            }
        }

        private bool _IsHighlighted()
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return false;

            if (_Interactions[_Selection].Type == EType.TSelectSlide)
                return _SelectSlides[_Interactions[_Selection].Num].Highlighted;

            return false;
        }

        private bool _IsVisible(int interaction)
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return false;

            switch (_Interactions[interaction].Type)
            {
                case EType.TButton:
                    return _Buttons[_Interactions[interaction].Num].Visible;

                case EType.TSelectSlide:
                    return _SelectSlides[_Interactions[interaction].Num].Visible;

                case EType.TStatic:
                    return _Statics[_Interactions[interaction].Num].Visible;

                case EType.TText:
                    return _Texts[_Interactions[interaction].Num].Visible;

                case EType.TSongMenu:
                    return _SongMenus[_Interactions[interaction].Num].Visible;

                case EType.TLyric:
                    return _Lyrics[_Interactions[interaction].Num].Visible;

                case EType.TNameSelection:
                    return _NameSelections[_Interactions[interaction].Num].Visible;

                case EType.TEqualizer:
                    return _Equalizers[_Interactions[interaction].Num].Visible;

                case EType.TPlaylist:
                    return _Playlists[_Interactions[interaction].Num].Visible;

                case EType.TParticleEffect:
                    return _ParticleEffects[_Interactions[interaction].Num].Visible;
            }

            return false;
        }

        private bool _IsEnabled(int interaction)
        {
            if (_Selection >= _Interactions.Count || _Selection < 0)
                return false;

            switch (_Interactions[interaction].Type)
            {
                case EType.TButton:
                    return _Buttons[_Interactions[interaction].Num].Enabled;

                case EType.TSelectSlide:
                    return _SelectSlides[_Interactions[interaction].Num].Visible;

                case EType.TStatic:
                    return false; //_Statics[_Interactions[interaction].Num].Visible;

                case EType.TText:
                    return false; //_Texts[_Interactions[interaction].Num].Visible;

                case EType.TSongMenu:
                    return _SongMenus[_Interactions[interaction].Num].Visible;

                case EType.TLyric:
                    return false; //_Lyrics[_Interactions[interaction].Num].Visible;

                case EType.TNameSelection:
                    return _NameSelections[_Interactions[interaction].Num].Visible;

                case EType.TEqualizer:
                    return false; //_Equalizers[_Interactions[interaction].Num].Visible;

                case EType.TPlaylist:
                    return _Playlists[_Interactions[interaction].Num].Visible;

                case EType.TParticleEffect:
                    return false; //_ParticleEffects[_Interactions[interaction].Num].Visible;
            }

            return false;
        }

        private SRectF _GetRect(int interaction)
        {
            SRectF Result = new SRectF();
            switch (_Interactions[interaction].Type)
            {
                case EType.TButton:
                    return _Buttons[_Interactions[interaction].Num].Rect;

                case EType.TSelectSlide:
                    return _SelectSlides[_Interactions[interaction].Num].Rect;

                case EType.TStatic:
                    return _Statics[_Interactions[interaction].Num].Rect;

                case EType.TText:
                    return _Texts[_Interactions[interaction].Num].Bounds;

                case EType.TSongMenu:
                    return _SongMenus[_Interactions[interaction].Num].Rect;

                case EType.TLyric:
                    return _Lyrics[_Interactions[interaction].Num].Rect;

                case EType.TNameSelection:
                    return _NameSelections[_Interactions[interaction].Num].Rect;

                case EType.TEqualizer:
                    return _Equalizers[_Interactions[interaction].Num].Rect;

                case EType.TPlaylist:
                    return _Playlists[_Interactions[interaction].Num].Rect;

                case EType.TParticleEffect:
                    return _ParticleEffects[_Interactions[interaction].Num].Rect;
            }

            return Result;
        }

        private void _AddInteraction(int num, EType type)
        {
            _Interactions.Add(new CInteraction(num, type));
            if (!_Interactions[_Selection].ThemeEditorOnly)
                _SetSelected();
            else
                _NextInteraction();
        }

        private void _DrawInteraction(int interaction)
        {
            switch (_Interactions[interaction].Type)
            {
                case EType.TButton:
                    _Buttons[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TSelectSlide:
                    _SelectSlides[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TStatic:
                    _Statics[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TText:
                    _Texts[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TSongMenu:
                    _SongMenus[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TNameSelection:
                    _NameSelections[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TEqualizer:
                    if (!_Equalizers[_Interactions[interaction].Num].ScreenHandles)
                    {
                        //TODO:
                        //Call Update-Method of Equalizer and give infos about bg-sound.
                        //_Equalizers[_Interactions[interaction].Num].Draw();
                    }
                    break;

                case EType.TPlaylist:
                    _Playlists[_Interactions[interaction].Num].Draw();
                    break;

                case EType.TParticleEffect:
                    _ParticleEffects[_Interactions[interaction].Num].Draw();
                    break;
                
                //TODO:
                //case EType.TLyric:
                //    _Lyrics[_Interactions[interaction].Num].Draw(0);
                //    break;
            }
        }

        #endregion InteractionHandling

        #region Theme Handling
        private void MoveElement(int stepX, int stepY)
        {
            if (_Interactions.Count > 0)
            {
                switch (_Interactions[_Selection].Type)
                {
                    case EType.TButton:
                        _Buttons[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TSelectSlide:
                        _SelectSlides[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TStatic:
                        _Statics[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TText:
                        _Texts[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TSongMenu:
                        _SongMenus[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TLyric:
                        _Lyrics[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TNameSelection:
                        _NameSelections[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TEqualizer:
                        _Equalizers[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TPlaylist:
                        _Playlists[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;

                    case EType.TParticleEffect:
                        _ParticleEffects[_Interactions[_Selection].Num].MoveElement(stepX, stepY);
                        break;
                }
            }
        }      

        private void ResizeElement(int stepW, int stepH)
        {
            if (_Interactions.Count > 0)
            {
                switch (_Interactions[_Selection].Type)
                {
                    case EType.TButton:
                        _Buttons[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TSelectSlide:
                        _SelectSlides[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TStatic:
                        _Statics[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TText:
                        _Texts[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TSongMenu:
                        _SongMenus[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TLyric:
                        _Lyrics[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TNameSelection:
                        _NameSelections[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TEqualizer:
                        _Equalizers[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TPlaylist:
                        _Playlists[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;

                    case EType.TParticleEffect:
                        _ParticleEffects[_Interactions[_Selection].Num].ResizeElement(stepW, stepH);
                        break;
                }
            }
        }

        private bool CheckVersion(int Version, XPathNavigator navigator)
        {
            int version = 0;
            CHelper.TryGetIntValueFromXML("//root/" + _ThemeName + "/ScreenVersion", navigator, ref version);

            if (version == Version)
                return true;
            else
            {
                string msg = "Can't load screen file of screen \"" + _ThemeName + "\", ";
                if (version < Version)
                    msg += "the file ist outdated! ";
                else
                    msg += "the file is for newer program versions! ";

                msg += "Current screen version is " + Version.ToString();
                _Base.Log.LogError(msg);
            }
            return false;
        }

        private void LoadThemeBasics(XPathNavigator navigator, int SkinIndex)
        {
            string value = String.Empty;

            // Backgrounds
            CBackground background = new CBackground(_Base, _PartyModeID);
            int i = 1;
            while (background.LoadTheme("//root/" + _ThemeName, "Background" + i.ToString(), navigator, SkinIndex))
            {
                AddBackground(background);
                background = new CBackground(_Base, _PartyModeID);
                i++;
            }  
            
            // Statics
            CStatic stat = new CStatic(_Base, _PartyModeID);
            i = 1;
            while (stat.LoadTheme("//root/" + _ThemeName, "Static" + i.ToString(), navigator, SkinIndex))
            {
                AddStatic(stat);
                stat = new CStatic(_Base, _PartyModeID);
                i++;
            }  

            // Texts
            CText text = new CText(_Base, _PartyModeID);
            i = 1;
            while (text.LoadTheme("//root/" + _ThemeName, "Text" + i.ToString(), navigator, SkinIndex))
            {
                AddText(text);
                text = new CText(_Base, _PartyModeID);
                i++;
            }

            // ParticleEffects
            CParticleEffect  partef = new CParticleEffect(_Base, _PartyModeID);
            i = 1;
            while (partef.LoadTheme("//root/" + _ThemeName, "ParticleEffect" + i.ToString(), navigator, SkinIndex))
            {
                AddParticleEffect(partef);
                partef = new CParticleEffect(_Base, _PartyModeID);
                i++;
            }  
        }

        private void ReloadThemeEditMode()
        {
            _Base.Theme.UnloadSkins();
            _Base.Theme.ListSkins();
            _Base.Theme.LoadSkins();
            _Base.Theme.LoadTheme();
            _Base.Graphics.ReloadTheme();

            OnShow();
            OnShowFinish();
        }
        #endregion Theme Handling
    }

    public class Basic
    {
        public IConfig Config;
        public ISettings Settings;
        public ITheme Theme;
        public IHelper Helper;
        public ILog Log;
        public IBackgroundMusic BackgroundMusic;
        public IDrawing Drawing;
        public IGraphics Graphics;
        public IFonts Fonts;
        public ILanguage Language;
        public IGame Game;
        public IProfiles Profiles;
        public IRecording Record;
        public ISongs Songs;
        public IVideo Video;
        public ISound Sound;
        public ICover Cover;
        public IDataBase DataBase;
        public IInputs Input;
        public IPlaylist Playlist;

        public Basic(IConfig Config, ISettings Settings, ITheme Theme, IHelper Helper, ILog Log, IBackgroundMusic BackgroundMusic,
            IDrawing Draw, IGraphics Graphics, IFonts Fonts, ILanguage Language, IGame Game, IProfiles Profiles, IRecording Record,
            ISongs Songs, IVideo Video, ISound Sound, ICover Cover, IDataBase DataBase, IInputs Input, IPlaylist Playlist)
        {
            this.Config = Config;
            this.Settings = Settings;
            this.Theme = Theme;
            this.Helper = Helper;
            this.Log = Log;
            this.BackgroundMusic = BackgroundMusic;
            this.Drawing = Draw;
            this.Graphics = Graphics;
            this.Fonts = Fonts;
            this.Language = Language;
            this.Game = Game;
            this.Profiles = Profiles;
            this.Record = Record;
            this.Songs = Songs;
            this.Video = Video;
            this.Sound = Sound;
            this.Cover = Cover;
            this.DataBase = DataBase;
            this.Input = Input;
            this.Playlist = Playlist;
        }
    }
}