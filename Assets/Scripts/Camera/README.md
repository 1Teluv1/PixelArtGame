# 역동적인 카메라 제어 시스템

이 시스템은 Unity에서 다양한 카메라 효과를 적용할 수 있는 모듈식 카메라 제어 시스템입니다.

## 기능

1. **카메라 흔들림 (Camera Shake)** - 충격이나 폭발 등을 표현하는 효과
2. **카메라 슬로우 (Slow Motion)** - 시간을 일시적으로 느리게 하는 효과
3. **카메라 블러 (Blur)** - 초점을 흐리게 하는 효과
4. **카메라 모자이크 (Mosaic)** - 화면을 픽셀화하는 효과

## 설정 방법

### 기본 설정

1. 플레이어의 카메라에 `CameraEffectsManager` 컴포넌트를 추가합니다.
2. 블러 효과를 사용하려면 `BlurMaterialCreator` 컴포넌트를 추가하거나, Custom/BlurEffect 셰이더로 만든 머티리얼을 수동으로 할당합니다.
3. 모자이크 효과를 사용하려면 `MosaicMaterialCreator` 컴포넌트를 추가하거나, Custom/MosaicEffect 셰이더로 만든 머티리얼을 수동으로 할당합니다.

### PlayerController 연동

`PlayerController` 클래스에 CameraEffectsManager 참조가 자동으로 추가되어 있습니다:

```csharp
[SerializeField] private CameraEffectsManager cameraEffects;
public CameraEffectsManager GetCameraEffects() => cameraEffects;
```

## 사용 방법

### 코드에서 사용하기

```csharp
// 카메라 흔들림
cameraEffects.ShakeCamera(intensity, duration);

// 슬로우 모션
cameraEffects.SlowMotion(timeScale, duration);

// 블러 효과
cameraEffects.ApplyBlur(intensity, duration);

// 모자이크 효과
cameraEffects.ApplyMosaic(pixelSize, duration);
```

### 이벤트에 연결하기

```csharp
// 예: 데미지를 받을 때 카메라 흔들림
public void TakeDamage(float damage)
{
    // 기존 데미지 로직...
    
    if (cameraEffects != null && damage > 0)
    {
        float intensity = Mathf.Clamp01(damage / maxHealth) * 0.3f;
        cameraEffects.ShakeCamera(intensity, 0.3f);
    }
}
```

## 데모 스크립트

`CameraEffectsDemo` 스크립트는 각 효과를 테스트하는 데 사용할 수 있습니다:

1. 게임 오브젝트에 `CameraEffectsDemo` 컴포넌트를 추가합니다.
2. 플레이어 컨트롤러를 할당합니다.
3. UI 버튼을 연결하거나 키보드 숫자키(1-5)를 사용하여 테스트합니다.

## 커스터마이징

인스펙터에서 각 효과의 기본값을 조정할 수 있습니다:

- 흔들림 강도 및 지속 시간
- 슬로우 모션 비율 및 지속 시간
- 블러 및 모자이크 강도

## 셰이더 설정

이 시스템은 두 개의 커스텀 셰이더를 사용합니다:

1. **Custom/BlurEffect** - 수평/수직 가우시안 블러를 적용하는 셰이더
2. **Custom/MosaicEffect** - 화면을 픽셀화하는 셰이더

각 셰이더는 Assets/Shaders 폴더에 있으며, 해당 MaterialCreator 스크립트에서 자동으로 로드됩니다.

## 사용 예시

```csharp
// 플레이어가 큰 데미지를 받을 때
if (damage > maxHealth * 0.3f)
{
    cameraEffects.ShakeCamera();  // 기본 설정으로 흔들림
    cameraEffects.ApplyBlur(5f, 0.3f);  // 약한 블러
}

// 플레이어가 사망했을 때
void OnDeath()
{
    cameraEffects.ShakeCamera(0.2f, 0.5f);
    cameraEffects.SlowMotion(0.2f, 1.0f);
    cameraEffects.ApplyBlur(15f, 0.8f);
    // 0.2초 후 모자이크 효과
    StartCoroutine(DelayedAction(() => cameraEffects.ApplyMosaic(32f, 0.5f), 0.2f));
} 