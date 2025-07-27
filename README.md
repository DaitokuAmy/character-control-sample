# character-control-sample

このプロジェクトは、**Unity** を使ったシンプルなキャラクター制御のサンプルプロジェクトです。

## 概要

- 主にキャラクターの移動・ジャンプ・攻撃などの基本的な操作を実装しています。
- サンプルシーン（`Assets/Scenes/sample.unity`）を起動し、**キーボードとマウス**でキャラクターを操作して挙動を検証できます。
- 入力には Unity Input System を利用しています。
- カメラ制御には Cinemachine を利用し、カメラの追従や回転もサンプルとして実装しています。

## 使い方

1. Unity で本プロジェクトを開きます。
2. `Assets/Scenes/sample.unity` をシーンとして開きます。
3. 再生（Play）ボタンを押すと、キーボードとマウスでキャラクターを操作できます。
   - **WASDキー**: キャラクターの移動
   - **スペースキー**: ジャンプ
   - **マウス移動**: カメラの回転
   - **マウス左クリック**: 攻撃

## 主要スクリプト

### `Player.cs`（`Assets/Scripts/Runtime/Player.cs`）

- キャラクターの移動・ジャンプ・攻撃など、プレイヤーの基本的な操作を管理する MonoBehaviour クラスです。
- 入力（Input System）を受け取り、アニメーターや CharacterController を通じてキャラクターを制御します。
- 主な機能:
  - **移動**: 入力方向に応じて速度を補間し、キャラクターを移動させます。
  - **回転**: カメラの向き（lookTransform）に合わせてキャラクターの向きを補間します。
  - **ジャンプ/攻撃**: 入力に応じてアニメーションステートを切り替えます。
  - **重力制御**: CharacterController の isGrounded を利用し、空中時は重力を加算します。
  - **アニメーター連携**: 速度やアクション状態を Animator パラメータに反映し、アニメーションと物理挙動を同期させます。
  - **OnAnimatorMove**: ルートモーションを利用した移動・回転の反映も対応。

### `CameraFollow.cs`（`Assets/Scripts/Runtime/CameraFollow.cs`）

- カメラの追従ターゲット（Cinemachine の Follow ターゲット）を制御する MonoBehaviour クラスです。
- 入力（Input System の Look アクション）に応じてターゲットの Y軸回転を制御し、カメラの回転を実現します。
- ターゲットの位置をキャラクターに追従させることで、Cinemachine カメラが自然にキャラクターを追いかける挙動を実現します。

## ファイル構成例

- `Assets/Scenes/sample.unity` : サンプルシーン
- `Assets/Scripts/Runtime/Player.cs` : プレイヤー制御スクリプト
- `Assets/Scripts/Runtime/CameraFollow.cs` : カメラ追従ターゲット制御スクリプト
- `Assets/Character/` : キャラクターのモデル・アニメーション・マテリアル等

---

本プロジェクトは、Unity でキャラクター制御やカメラ制御の基本を学びたい方に最適なサンプルです。自由に改変してご利用ください。
