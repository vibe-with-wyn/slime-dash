# Slime Dash 🎮

[![Play on itch.io](https://img.shields.io/badge/Play-itch.io-red?style=for-the-badge&logo=itch.io)](https://vibe-with-wyn.itch.io/slime-dash)
[![Unity](https://img.shields.io/badge/Unity-2021+-blue?style=for-the-badge&logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

> **A colorful 2D mobile platformer featuring precise jumping, coin collection, and challenging level progression**

---

## 🎮 About

**Slime Dash** is a fast-paced 2D platformer where you guide a brave slime through dangerous levels filled with spikes, platforms, and collectible coins. Master the controls, avoid hazards, and reach the portal to unlock new challenges!

### Game Features
- 🎯 **Progressive Level System** with unlockable stages
- 💰 **Coin Collection** with persistent score tracking
- ❤️ **Lives System** with checkpoint-based respawning
- 🎨 **Smooth Animations** - Idle, jump, die, swallow states
- 🎵 **Dynamic Sound Effects** for all actions
- 📱 **Mobile-Optimized** for landscape play (1920×1080)
- 🏁 **Portal Completion** with satisfying animations
- 💾 **Auto-Save Progress** - your progress is always saved

---

## 📱 Play Now

### WebGL (Browser)
**[🎮 Play on itch.io](https://vibe-with-wyn.itch.io/slime-dash)**

- ✅ No installation required
- ✅ Works on desktop and mobile browsers
- ✅ Instant play - loads in seconds
- ⚡ Hardware acceleration recommended

### Android APK (Coming Soon)
- Download link will be available in [Releases](https://github.com/vibe-with-wyn/slime-dash/releases)
- Minimum Android 7.0 (API 24)
- 150MB free storage

---

## 🕹️ Controls

### Mobile (Touch)
- **Move:** Tap left/right on-screen buttons
- **Jump:** Tap jump button
- **Navigate Menus:** Tap UI elements

**Tip:** Touch controls are optimized for landscape orientation!

---

## 🎯 How to Play

1. **Navigate the Levels** - Use precise jumps to cross platforms
2. **Collect Coins** - Gather all coins for maximum score
3. **Avoid Hazards** - Spikes and death zones will cost you a life
4. **Reach the Portal** - Complete each level to unlock the next
5. **Manage Lives** - You have 3 hearts - use them wisely!

### Game Progression
- **Level 1:** Tutorial - Learn the basics
- **Level 2+:** Unlocked by completing previous levels
- **Checkpoints:** Auto-respawn at checkpoints after death
- **Game Over:** Retry or return to level selection

---

## 🎨 Game Systems

### Player Mechanics
- **Responsive Controls** - Tight platformer feel with precise jumping
- **Ground Detection** - Tag-based collision with overlap fallback
- **Animator States** - IsGrounded, IsJumping, IsMoving
- **Input Locking** - Disables during cutscenes and death

### Lives & Respawn
- **3 Hearts** - Displayed as sprite hearts in the UI
- **Checkpoint System** - Respawn at last checkpoint
- **Death Animation** - Smooth death sequence before respawn
- **Game Over Panel** - Retry button to restart level

### Sound System
- **Action SFX** - Jump, die, coin collect, movement
- **Button Feedback** - Selection sounds on hover/click
- **Looping Movement** - Continuous audio while moving
- **Mobile Audio Unlock** - Automatically unlocked on first tap

### Level Progression
- **Portal Completion** - Enter portal to finish level
- **Unlock Next Level** - Progression tracked via GameManager
- **Level Selection UI** - Visual lock/unlock states
- **Persistent Progress** - Saved via PlayerPrefs

---

## 📊 Technical Details

| Specification | Details |
|---------------|---------|
| **Engine** | Unity 2021+ |
| **Language** | C# 9.0 (.NET Framework 4.7.1) |
| **Platforms** | WebGL, Android (Planned) |
| **Resolution** | 1920×1080 (Landscape) |
| **WebGL Size** | ~50 MB (Brotli compressed) |
| **Testing** | NUnit - 12+ unit tests |

### Browser Requirements
- Modern browser (Chrome 90+, Firefox 88+, Edge 90+, Safari 14+)
- WebGL 2.0 support (WebGL 1.0 fallback available)
- Hardware acceleration enabled
- 512 MB WebGL memory (configurable)

---

## 🛠️ Development

### Key Systems

#### Player Controller
- Tag-based ground checking with overlap detection
- Touch and keyboard input support
- Animator state management
- Input enable/disable for cutscenes

#### Game Manager (Singleton)
- Level unlock/completion tracking via PlayerPrefs
- Persistent coin collection system
- Scene-wide progress management

#### Lives & Respawn
- Heart-based lives display with sprite toggling
- Checkpoint system with configurable respawn points
- Game Over panel with retry functionality
- Integration with RespawnManager

#### Sound System
- `SoundEffectController` - Detects animator states and plays SFX
- `ButtonSelectSfx` - Plays selection sounds on UI interactions
- Audio unlocking for mobile browsers

---

## 🏗️ Building from Source

### Prerequisites
- Unity 2021.3 LTS or newer
- WebGL Build Support module installed

### WebGL Build
1. Clone this repository:

2. Open project in Unity

3. Configure **Edit > Project Settings > Player > WebGL**:
   - Company Name: `VibWithWyn`
   - Product Name: `Slime Dash`
   - WebGL Memory Size: `512` MB
   - Compression Format: `Brotli`
   - Enable Decompression Fallback: ✅

4. **File > Build Profiles** → Select **Web** → **Build**

5. Zip the build folder (ensure `index.html` is at root)

6. Upload to itch.io or host on Netlify/GitHub Pages

### Android Build (Planned)
1. Install **Android Build Support** module
2. Switch Platform to **Android**
3. Configure:
   - Bundle Identifier: `com.vibwithwyn.slimedash`
   - Minimum API Level: `24`
   - Orientation: `Landscape`
4. **Build** → Save as `.apk`

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Add unit tests for new features
- Update documentation as needed
- Test on multiple browsers/devices before submitting

---

## 📜 License

Licensed under the **MIT License** - see [LICENSE](LICENSE) file for details.

### Third-Party Assets
- **TextMesh Pro** - © Unity Technologies (Unity Companion License)
- **EmojiOne** - © EmojiOne (see attribution file)
- **Liberation Sans Font** - SIL Open Font License 1.1

---

## 🎨 Credits

### Development
- **Developer:** Wendel ([vibe-with-wyn](https://github.com/vibe-with-wyn))
- **Engine:** Unity Technologies
- **Programming:** C#

### Game Assets
Third-party assets used under their respective licenses. Credits to the owner of all the free game assets that I did use on this game


### Tools & Assistance
- Unity Engine © Unity Technologies
- AI Development Assistance: GitHub Copilot
- Testing Framework: NUnit

---

## 🐛 Issues & Support

- **Report Bugs:** [GitHub Issues](https://github.com/vibe-with-wyn/slime-dash/issues)
- **Feature Requests:** [GitHub Discussions](https://github.com/vibe-with-wyn/slime-dash/discussions)
- **Feedback:** Leave a comment on [itch.io](https://vibe-with-wyn.itch.io/slime-dash)

---

## 🌟 Show Your Support

If you enjoy Slime Dash:
- ⭐ Star this repository
- 🍴 Fork and contribute improvements
- 📢 Share with friends and on social media
- 💬 Rate the game on [itch.io](https://vibe-with-wyn.itch.io/slime-dash)
- 🎮 Follow development updates

---

## 📈 Roadmap

### Planned Features
- 🎮 **More Levels** - 10+ additional stages
- 🎨 **New Slime Skins** - Unlockable character variants
- 🏆 **Leaderboards** - Global high scores
- 🎵 **Music System** - Background music tracks
- 📱 **Android APK Release** - Downloadable mobile version
- 🌐 **Localization** - Multi-language support

### Known Improvements
- Performance optimization for mobile browsers
- Additional animation polish
- Enhanced tutorial/onboarding
- More sound effects and visual feedback

---

<div align="center">

**Keep jumping, avoid spikes, and dash to victory!** 🎮✨

[Play Now](https://vibe-with-wyn.itch.io/slime-dash) • [Report Bug](https://github.com/vibe-with-wyn/slime-dash/issues) • [Star Repo](https://github.com/vibe-with-wyn/slime-dash)

Made with ❤️ by [Wendel](https://github.com/vibe-with-wyn)

</div>
