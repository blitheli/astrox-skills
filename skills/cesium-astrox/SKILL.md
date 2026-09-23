---
name: cesium-astrox
description: ASTROX 扩展版 Cesium.js (Cesium-Astrox) 的场景与图层用法。用户需要多天体 SolarSystem 场景、中央天体、addViewer、行星实体、CzmlDataSource/VGT、分辨率、回退 Viewer,或为地球/月球创建影像与地形图层时使用;也用于询问 ASTROX 定制 Cesium 与原版差异。场景初始化时 AstroxWasm.setDotnetUrl 为必选,失败则中止;VGT 与 Planetary 仍为可选。不适用于仅讨论原版 Cesium 的一般问题。
---

# ASTROX 扩展 Cesium (Cesium-Astrox)

本技能覆盖 **ASTROX 对 Cesium.js 的扩展与定制**。其 API、约定与运行时行为可能与官方/原版 Cesium 不同,不可直接套用通用 Cesium 文档或社区示例。

多天体场景与地球/月球影像、地形以本文对应章节为准。本文未写到的 ASTROX API 不要臆造。

## 核心指令 (Core Instructions)

1. **优先本技能**:编写或修改 Cesium-Astrox 场景时,按下面的调用顺序使用已记录的 API 与参数。不要改成原版 `Cesium.Viewer` 单地球写法,除非 SolarSystem 初始化失败、需要走回退 Viewer。
2. **区分扩展与原版**:仅当用户明确只讨论原版 Cesium、且与 ASTROX 扩展无关时,才可参考通用 Cesium 文档。
3. **场景与图层已入库**:多天体场景创建、分辨率、必选 AstroxWasm、可选 VGT/Planetary、回退 Viewer,以及地球/月球影像与地形,按本文章节实现。其他专题仍只使用 `examples/`、`notes/` 里已经出现的写法。

## 目录结构 (可扩展)

```text
skills/cesium-astrox/
  SKILL.md          # 本说明
  examples/         # 示例片段、演示入口 (优先引用)
  notes/            # 补充说明、差异备注、专题笔记
```

## 初始化顺序

`initViewer(onReady)` 固定为四步:

1. `await` 必选 `Cesium.AstroxWasm.setDotnetUrl`。失败则中止 `initViewer`,不要继续创建 SolarSystem,也不要用回退 Viewer 掩盖这次失败。
2. `await` 可选插件初始化(VGT、Planetary)。任一步失败只警告,继续后面的场景。
3. `try` 创建多天体 `Cesium.SolarSystem`,在 `centralBodiesLoaded` 里完成 Viewer、行星、CZML/VGT、影像和地形,然后调用 `onReady`。
4. `catch`:若此时还没有 viewer,创建回退 `Cesium.Viewer` 再调用 `onReady`。SolarSystem 已交出 viewer 时不要再盖一层回退 Viewer。

## AstroxWasm(必选)

场景创建前必须完成,不要放进「失败则跳过」的 `try/catch`。

```javascript
await Cesium.AstroxWasm.setDotnetUrl("/dotnet/_framework/dotnet.js");
```

`Cesium.AstroxWasm` 或 `setDotnetUrl` 不存在,或调用抛错时,停止初始化并向调用方抛出错误。

## 可选插件初始化

VGT 与 Planetary 互相独立,各自 `try/catch`。对象或方法不存在时跳过。

1. VGT:`Cesium.VGT.install(Cesium.buildModuleUrl("Assets/data/VGT/VGTConfig.json"))`
2. Planetary:

```javascript
await Cesium.Planetary.install({
    url: Cesium.buildModuleUrl("Assets/data/CentralBodies/"),
    data: ["Earth", "Moon", "Sun"]
});
```

`Planetary.install` 的 `data` 含 `Sun`。后面 `SolarSystem` 的 `centralBodies.data` 只放要作为中央天体加载的 `Earth` 与 `Moon`,两者不要混成同一份列表。

## 多天体场景创建

### 构造 SolarSystem

构造时就把降采样关掉,`addViewer` 才会继承该配置。

```javascript
const solarSystem = new Cesium.SolarSystem({
    isMultiCentralBody: true,
    useBrowserRecommendedResolution: false,
    msaaSamples: 4,
    centralBodies: {
        url: Cesium.buildModuleUrl("Assets/data/CentralBodies/"),
        data: ["Earth", "Moon"]
    }
});
```

| 字段 | 值 | 作用 |
| :--- | :--- | :--- |
| `isMultiCentralBody` | `true` | 多中央天体场景 |
| `useBrowserRecommendedResolution` | `false` | 用设备像素比渲染,避免高分屏发糊 |
| `msaaSamples` | `4` | 多重采样 |
| `centralBodies.url` | `Cesium.buildModuleUrl("Assets/data/CentralBodies/")` | 中央天体数据包 |
| `centralBodies.data` | `["Earth", "Moon"]` | 要加载的中央天体 |

后续 Viewer、行星、数据源、影像和地形都放在 `solarSystem.centralBodiesLoaded` 的监听里,不要在天体加载完成前调用。

### addViewer

监听回调内,以太阳为中心添加 Viewer。容器 id 使用页面上的 `cesiumContainer`。

```javascript
const viewerKey = solarSystem.addViewer(
    "cesiumContainer",
    {
        ellipsoid: Cesium.EllipsoidStore.EARTH,
        planetaries: "all"
    },
    {
        navigation: true,
        keyboardRoaming: false,
        inspector: false
    }
);

const viewer = solarSystem.getViewer(viewerKey) || solarSystem.baseViewer;
```

优先 `getViewer(viewerKey)`。`baseViewer` 可能尚未就绪,只作回退。拿到 viewer 后立刻做一次分辨率设置,再 `solarSystem.flyToPlanetary(viewerKey, "Earth")`。

`showPlanetaries`、`showOrbit`、`showLabel` 不在这条初始化路径里调用。需要整屏开关时再单独使用,不要在创建场景时默认打开。

### addPlanetary

行星实体单独创建,便于之后改 `showLabel`、`color`、`showOrbit`。

```javascript
function addPlanetary(cbName, color = Cesium.Color.WHITE, maxDistance = 1e13) {
    const planetary = Cesium.Planetary.getPlanetary(cbName);
    planetary.showOrbit = true;
    planetary.showLabel = true;
    planetary.color = color;
    planetary.label.distanceDisplayCondition = new Cesium.DistanceDisplayCondition(10000000.0, maxDistance);
    return solarSystem.addEntity(planetary);
}
```

月球调用:`addPlanetary("Moon", Cesium.Color.WHITE, 7e9)`。`DistanceDisplayCondition` 近距为 `10000000.0`,远距为传入的 `maxDistance`。

除行星实体外,新建实体放进下面的 `CzmlDataSource.entities`,不要直接塞进行星集合。

### CzmlDataSource 与 VGT

VGT 元素必须挂在 `CzmlDataSource` 上,并交给 `SolarSystem`。

```javascript
const czmlDataSource = new Cesium.CzmlDataSource();
czmlDataSource.addVGTChange();
czmlDataSource.referenceFrame = Cesium.ReferenceFrame.INERTIAL;
solarSystem.addDataSource(czmlDataSource);
```

顺序固定:`new Cesium.CzmlDataSource` → `addVGTChange()` → `referenceFrame = Cesium.ReferenceFrame.INERTIAL` → `solarSystem.addDataSource`。

### 分辨率

Cesium 的像素比是:`useBrowserRecommendedResolution` 为 true 时取 1,否则取 `devicePixelRatio`,再乘 `resolutionScale`。因此:

- `SolarSystem` 与 `Viewer` 都设 `useBrowserRecommendedResolution = false`
- `viewer.resolutionScale = 1`(不要再乘一次 dpr,否则变成 dpr 的平方)
- `solarSystem.msaaSamples` 与 `viewer.scene.msaaSamples` 为 4
- `viewer.scene.postProcessStages.fxaa.enabled = true`(包在 try 里,失败只警告)
- `viewer.scene.requestRenderMode = false`,然后 `viewer.resize()` 与 `viewer.scene.requestRender()`

调用两次:viewer 到手后一次,影像与地形加完后用 `requestAnimationFrame` 再强制一次。SolarSystem 后续同步可能把分辨率写回去。

### 回退 Viewer

仅当 SolarSystem 抛错且 viewer 仍为空时使用。这是标准 Cesium Viewer,不是多天体场景。

```javascript
const viewer = new Cesium.Viewer("cesiumContainer", {
    animation: false,
    timeline: false,
    baseLayerPicker: false,
    geocoder: false,
    homeButton: false,
    imageryProvider: false,
    navigationHelpButton: false,
    fullscreenButton: false,
    sceneModePicker: false,
    useBrowserRecommendedResolution: false,
    skyBox: false
});
```

创建后同样做分辨率设置,再 `viewer.camera.flyTo({ destination: Cesium.Cartesian3.fromDegrees(120, 20, 3.5e7), duration: 1.5 })`。

## 影像图层与地形图层

在 `centralBodiesLoaded` 内、Viewer 与 CzmlDataSource 就绪之后添加。多天体场景用 `SolarSystem` 的配置接口挂到指定天体,不要只写到当前 `viewer.imageryLayers`。

天体名用 `"Earth"` 或 `"Moon"`。椭球与该天体一致。极区瓦片必须带 `pole`(南极 `"South"`)。

顺序:

1. 构造影像 provider。XYZ 瓦片用 `UrlTemplateImageryProvider`,`tilingScheme` 为 `GeographicTilingScheme`,其 `ellipsoid` 与天体一致,并设置 `maximumLevel`。
2. `Cesium.ImageryLayer.fromProviderAsync(provider, { pole })` 得到图层。极区数据传入 `pole`。
3. `solarSystem.configAddImageLayer(cbName, imageryLayer)`。
4. `new Cesium.CesiumTerrainProvider({ url, ellipsoid, pole })`。
5. `solarSystem.configAddTerrainProvider(cbName, terrainProvider)`。

### 月球南极影像

```javascript
const moonImagery = new Cesium.UrlTemplateImageryProvider({
    url: "http://www.astrox.cn:8767/xyz/LRO_WAC_Mosaic_SPole60_100mp/{z}/{x}/{y}",
    tilingScheme: new Cesium.GeographicTilingScheme({
        ellipsoid: Cesium.Ellipsoid.MOON
    }),
    maximumLevel: 10
});
const moonImageryLayer = Cesium.ImageryLayer.fromProviderAsync(moonImagery, { pole: "South" });
solarSystem.configAddImageLayer("Moon", moonImageryLayer);
```

### 月球南极地形

```javascript
const moonTerrain = new Cesium.CesiumTerrainProvider({
    url: "http://www.astrox.cn:8766/v1/tilesets/LDEM_80S_20M_lbl/tiles",
    ellipsoid: Cesium.Ellipsoid.MOON,
    pole: "South"
});
solarSystem.configAddTerrainProvider("Moon", moonTerrain);
```

影像与地形成对出现:同一天体、同一 `pole`、同一椭球 `Cesium.Ellipsoid.MOON`。先影像,后地形。

### 地球影像

地球不在场景初始化里写死底图。需要 OSM 时按需添加,并避免重复挂层。

```javascript
const osmLayer = new Cesium.ImageryLayer(new Cesium.OpenStreetMapImageryProvider({
    url: "https://tile.openstreetmap.org/"
}));
osmLayer.sortIndex = 10;

const layers = solarSystem.planetaries?.Earth?.imageryLayers;
if (layers?.indexOf(osmLayer) === -1) {
    solarSystem.configAddImageLayer("Earth", osmLayer);
}
```

显隐同时改 `osmLayer.show` 和 `osmLayer.cloneObjects` 里每一项的 `show`,否则多窗口克隆层不会一起变。

地球若改用 XYZ 瓦片,仍走 `UrlTemplateImageryProvider` + `configAddImageLayer("Earth", layer)`,`GeographicTilingScheme` 的椭球用地球椭球,不要用 `Cesium.Ellipsoid.MOON`。地球地形同样调用 `configAddTerrainProvider("Earth", terrainProvider)`。本技能没有单独的地球地形 URL,不要编造 tileset 地址;有用户给出的地球地形地址时,再填 `url`,并让 `ellipsoid` 与地球一致。极区地球瓦片同样传 `pole`。

## 场景内推荐调用顺序

`centralBodiesLoaded` 回调内保持这一顺序:

1. `addViewer`
2. `getViewer` 或 `baseViewer`,并做分辨率设置
3. `flyToPlanetary(viewerKey, "Earth")`
4. `addPlanetary` 放入需要单独控制的行星
5. 创建 `CzmlDataSource`, `addVGTChange`, 设 `referenceFrame`, `addDataSource`
6. `configAddImageLayer`,再 `configAddTerrainProvider`
7. 下一帧再做一次分辨率设置
8. `onReady()`

## 注意事项

- AstroxWasm 初始化失败则中止,不创建 SolarSystem,也不回退 Viewer。VGT、Planetary 初始化失败不阻断场景。SolarSystem 构造失败才走回退 Viewer。
- 分辨率要在 `SolarSystem` 和 `Viewer` 两侧一起关 `useBrowserRecommendedResolution`, `resolutionScale` 保持 1,并在下一帧再写一次。
- 行星走 `Planetary.getPlanetary` + `solarSystem.addEntity`。VGT 与其他新建实体走 `CzmlDataSource`。
- 影像、地形挂到天体名 `"Earth"` 或 `"Moon"`,极区必须带 `pole`,椭球与天体一致。
- 月球南极示例的影像服务是 `astrox.cn:8767`,地形服务是 `astrox.cn:8766`。换数据集时只替换 `url`、`maximumLevel` 与 `pole`,不要改调用顺序。
- 本文未出现的 ASTROX Cesium API 先查 `examples/` 与 `notes/`,没有则向用户确认,不要按原版 Cesium 猜测扩展参数。

## 标准执行流程

1. 确认问题是否涉及 ASTROX 扩展 Cesium。多天体场景、行星实体、VGT、地球或月球影像/地形属于本技能。
2. 场景创建按「初始化顺序」和「场景内推荐调用顺序」写,参数用本文表格与代码中的值。先完成必选 AstroxWasm,再装可选的 VGT 与 Planetary。
3. 图层按「影像图层与地形图层」选择天体。月球南极用已给出的 URL、`Ellipsoid.MOON` 和 `pole: "South"`。地球 OSM 按需添加并同步 `cloneObjects`。
4. 本文没有的 API 或地形地址,说明资料尚未入库,不要编造。
