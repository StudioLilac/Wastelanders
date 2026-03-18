using Systems.Persistence;

namespace Managers {
    public class PreferencesManager : PersistentSingleton<PreferencesManager>
    {
        private UserPreferences userPreferences = null!;

        protected override void Awake()
        {
            base.Awake();
            Bind(SaveLoadSystem.Instance.GetUserPreferences());
        }

        void Bind(UserPreferences preferences)
        {
            userPreferences = preferences;
            CardComparator.autoRoll = preferences.AutoRollEnabled;
            ScreenShakeHandler.IsScreenShakeEnabled = preferences.screenShakePreference.IsScreenShakeEnabled;
            
            AudioPreferences audioPreferences = preferences.audioPreferences;
            AudioManager.Instance.SetMusicVolume(audioPreferences.BackgroundMusicVolume);
            AudioManager.Instance.SetSFXVolume(audioPreferences.SFXVolume);
            AudioManager.Instance.SetMusicMuted(audioPreferences.MusicMuted);
            AudioManager.Instance.SetSFXMuted(audioPreferences.SFXMuted);
        }

        public void SetAutoRoll(bool value) {
            CardComparator.autoRoll = value;
            userPreferences.AutoRollEnabled = value;
            
            SaveLoadSystem.Instance.SavePreferences(); 
        }
        
        public void SetDoubleSpeed(bool value) {
            userPreferences.DoubleSpeedEnabled = value;
            
            SaveLoadSystem.Instance.SavePreferences(); 
        }

        public void SetScreenShakeEnabled(bool value) {
            ScreenShakeHandler.IsScreenShakeEnabled = value;
            userPreferences.screenShakePreference.IsScreenShakeEnabled = value;
        }

        public void SetSFXVolume(float volume) {
            userPreferences.audioPreferences.SFXVolume = volume;
            AudioManager.Instance.SetSFXVolume(volume);
        }
        
        public void SetMusicVolume(float volume) {
            userPreferences.audioPreferences.BackgroundMusicVolume = volume;
            AudioManager.Instance.SetMusicVolume(volume);
        }
        
        public void SetSFXMuted(bool muted) {
            userPreferences.audioPreferences.SFXMuted = muted;
            AudioManager.Instance.SetSFXMuted(muted);
        }
        
        public void SetMusicMuted(bool muted) {
            userPreferences.audioPreferences.MusicMuted = muted;
            AudioManager.Instance.SetMusicMuted(muted);
        }

        public float GetMusicVolume() {
            return userPreferences.audioPreferences.BackgroundMusicVolume;
        }
        
        public float GetSFXVolume() {
            return userPreferences.audioPreferences.SFXVolume;
        }
    }
}