---
name: cesium-astrox
description: ASTROX 扩展版 Cesium.js (Cesium-Astrox) 的场景与图层用法。用户需要多天体 SolarSystem 场景、中央天体、addViewer、行星实体、CzmlDataSource/VGT、分辨率、回退 Viewer,为地球/月球创建影像与地形图层,用 CZML position 创建带 centralBody 的轨迹 Entity,在 path.groundTracks 上显示地面轨迹,或用 entity.orbits 显示多条相关轨迹时使用;也用于询问 ASTROX 定制 Cesium 与原版差异。场景初始化时 AstroxWasm.setDotnetUrl 为必选,失败则中止;VGT 与 Planetary 仍为可选。不适用于仅讨论原版 Cesium 的一般问题。
---

# ASTROX 扩展 Cesium (Cesium-Astrox)

本技能覆盖 **ASTROX 对 Cesium.js 的扩展与定制**。其 API、约定与运行时行为可能与官方/原版 Cesium 不同,不可直接套用通用 Cesium 文档或社区示例。

多天体场景与地球/月球影像、地形以本文对应章节为准。本文未写到的 ASTROX API 不要臆造。

## 核心指令 (Core Instructions)

1. **优先本技能**:编写或修改 Cesium-Astrox 场景时,按下面的调用顺序使用已记录的 API 与参数。不要改成原版 `Cesium.Viewer` 单地球写法,除非 SolarSystem 初始化失败、需要走回退 Viewer。
2. **区分扩展与原版**:仅当用户明确只讨论原版 Cesium、且与 ASTROX 扩展无关时,才可参考通用 Cesium 文档。
3. **场景与图层已入库**:多天体场景创建、分辨率、必选 AstroxWasm、可选 VGT/Planetary、回退 Viewer、地球/月球影像与地形、用 CZML position 创建轨迹 Entity、path.groundTracks 地面轨迹,以及 entity.orbits 多条相关轨迹,按本文章节实现。其他专题仍只使用 `examples/`、`notes/` 里已经出现的写法。

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

## 用 CZML position 创建轨迹 Entity

行星体走上一节的 `addPlanetary` 与 `solarSystem.addEntity`。轨道、航天器等其它实体不要放进 `viewer.entities`,也不要 `solarSystem.addEntity`。最基础的输入是 `Cesium.CzmlPosition`(或字段相同的普通对象 / 对象数组)。积分器和 Web API 只负责产出这个包,创建 Entity 都走 `createEntityFromCzmlPosition`,再 `czmlDataSource.entities.add`。

`entityOptions` 里不要传 `position`。写入 Entity 前会丢掉它。外观(id、name、path、point、label)和 `centralBody` 放在选项里,位置单独传入。

`centralBody` 是该轨迹的中心天体名。月球轨道用 `"Moon"`,日心小行星用 `"Sun"`。漏掉它时轨迹不会挂到对应天体上。

```javascript
function createEntityFromCzmlPosition(czmlPositionData, entityOptions = {}) {
    const { position: _ignored, ...rest } = entityOptions;
    const entity = new Cesium.Entity(rest);

    // 多段 position 走 packet 数组,单段走一个 position 包
    if (Array.isArray(czmlPositionData)) {
        Cesium.CzmlDataSource.processPositionPacketData(entity, "position", czmlPositionData);
    } else {
        Cesium.CzmlDataSource.processPositionProperty(entity, "position", czmlPositionData);
    }

    if (!entity.position) {
        throw new Error("CZML position 解析失败,未得到 PositionProperty");
    }
    return entity;
}
```

同一 `id` 再次加入前,先从 `czmlDataSource.entities` 按 id 移除,避免 “entity already exists”。

### 直接用 CzmlPosition

`new Cesium.CzmlPosition(options)` 与 API / JSON 里的 position 包是同一套字段。`processPositionProperty` 只读字段,两种都可以传入。省略 `referenceFrame` 时构造函数默认 `"FIXED"`。惯性轨道必须写成 `"INERTIAL"`。`epoch` 用 ISO8601,不要依赖构造函数里的默认历元。

三个采样数组只设一个:

| 字段 | 步长 | 每个采样 |
| :--- | :--- | :--- |
| `cartesian` | 4 | 相对 epoch 的秒, `x`, `y`, `z`(米) |
| `cartesianVelocity` | 7 | 秒, `x`, `y`, `z`, `vx`, `vy`, `vz` |
| `cartographicDegrees` | 4 | 秒, 经度(度), 纬度(度), 高(米) |

可选 `interval`(`"start/stop"`)、`interpolationAlgorithm`(`"LAGRANGE"` 或 `"HERMITE"`)、`interpolationDegree`。未传时算法默认 `LAGRANGE`,阶数默认 `5`。包里已有这些字段时原样传入,不要改成默认值。

```javascript
const czmlPosition = new Cesium.CzmlPosition({
    epoch: "2020-11-23T21:06:35.761Z",
    referenceFrame: "INERTIAL",
    interpolationAlgorithm: "LAGRANGE",
    interpolationDegree: 5,
    cartesian: [
        0, -1808774.52236705, -6210104.58567043, -2393122.32753525,
        600, 3889226.541885434, -7756350.41535623, -3083645.5469023483
    ]
});

const visual = {
    id: "czml-orbit",
    name: "CZML 轨道",
    centralBody: "Earth",
    path: {
        show: true,
        width: 2,
        resolution: 60,
        leadTime: 1e10,
        trailTime: 1e10,
        material: Cesium.Color.CYAN
    },
    point: {
        pixelSize: 8,
        color: Cesium.Color.CYAN,
        outlineColor: Cesium.Color.WHITE,
        outlineWidth: 1
    },
    label: {
        text: "CZML 轨道",
        font: "16px Microsoft YaHei",
        fillColor: Cesium.Color.WHITE,
        outlineColor: Cesium.Color.BLACK,
        outlineWidth: 2,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        pixelOffset: new Cesium.Cartesian2(0, -22)
    }
};

const existing = czmlDataSource.entities.getById(visual.id);
if (existing) {
    czmlDataSource.entities.remove(existing);
}
czmlDataSource.entities.add(createEntityFromCzmlPosition(czmlPosition, visual));
```

多段轨迹传入 `CzmlPosition` 数组(例如接口里的 `Positions.CzmlPositions`)。每一段自己带 `interval`。函数看到数组会走 `processPositionPacketData`。不要把数组再包进一个新的 `CzmlPosition`。已有普通对象、字段与上表一致时,直接传入,不必再 `new Cesium.CzmlPosition`。

### 地月转移:地面轨迹与 orbits

主轨迹仍用上一节的 `createEntityFromCzmlPosition`。地月转移的 `centralBody` 是 `"Earth"`。投影到另一天体表面的地面轨迹写在 `path.groundTracks`,不要为此再创建一个 Entity。每一项只有 `centralBody` 和 `show`。要画在月球表面上时用 `"Moon"`。

多条相关轨迹(例如一组方位角)不要各自 `entities.add`,也不要写进主 Entity 的 `path`。在加入数据源之前赋给 `entity.orbits`。

```javascript
const packets = data.Positions.CzmlPositions;
const visual = {
    id: "earth2moon-demo",
    name: "地月转移",
    centralBody: "Earth",
    path: {
        show: true,
        width: 2,
        resolution: 120,
        leadTime: 1e10,
        trailTime: 1e10,
        material: Cesium.Color.CYAN,
        groundTracks: [
            {
                centralBody: "Moon",
                show: true
            }
        ]
    },
    point: {
        pixelSize: 8,
        color: Cesium.Color.CYAN,
        outlineColor: Cesium.Color.WHITE,
        outlineWidth: 1
    },
    label: {
        text: "地月转移",
        font: "16px Microsoft YaHei",
        fillColor: Cesium.Color.WHITE,
        outlineColor: Cesium.Color.BLACK,
        outlineWidth: 2,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        pixelOffset: new Cesium.Cartesian2(0, -22)
    }
};

const existing = czmlDataSource.entities.getById(visual.id);
if (existing) {
    czmlDataSource.entities.remove(existing);
}
const entity = createEntityFromCzmlPosition(packets, visual);

function orbitFromPacket(name, packet) {
    const interval = packet.interval || packet.Interval || "";
    const [start, stop] = String(interval).split("/");
    const startIso = start || packet.epoch || packet.Epoch || "";
    return {
        enabled: true,
        name,
        startTime: startIso ? Cesium.JulianDate.fromIso8601(startIso) : undefined,
        stopTime: stop ? Cesium.JulianDate.fromIso8601(stop) : undefined,
        position: new Cesium.CzmlPositionHelper(packet).getPositionProperty(),
        size: 6,
        outlineColor: new Cesium.ConstantProperty(Cesium.Color.WHITE),
        outlineWidth: 2,
        positionType: "CzmlPosition",
        width: 2,
        resolution: 120,
        system: "Inertial"
    };
}

entity.orbits = new Cesium.OrbitsGraphics({
    show: true,
    showLabels: true,
    showPoints: true,
    data: relatedPackets.map((packet, index) => orbitFromPacket(`Az${index}`, packet))
});
czmlDataSource.entities.add(entity);
```

`relatedPackets` 里每一项是一条相关轨迹的单个 `CzmlPositions[0]`。`system` 固定 `"Inertial"`。写成 `"Earth"` 或 `"Moon"` 后,切换中心天体时这些轨迹不会跟着变。`positionType` 固定 `"CzmlPosition"`。`position` 必须是 `CzmlPositionHelper.getPositionProperty()` 的返回值,不要把原始包直接塞进 `data`。

`OrbitsGraphics` 的 `show`、`showLabels`、`showPoints` 分别控制整组轨迹、标签和点。赋值发生在 `entities.add` 之前。

主轨迹的时钟用第一段的 `interval`(`"start/stop"`)对齐 `viewer.clock`。没有 `interval` 时,起点回退到该段 `epoch`。

### 月球二体轨道

近月圆轨道用 `Cesium.TwoBodyPropagator` 采样,再交给上面的函数。`toCzmlPosition(start, stop, step)` 返回 `CzmlPositionHelper`,必须再调 `getCzmlPostions()`(方法名就是这个拼写)。无分段时返回单个 position 包,存在 `boundaryTimes` 时返回数组。

`toCzmlPosition` 已把 `referenceFrame` 写成 `"INERTIAL"`。单个包可以再赋一次 `"INERTIAL"`。返回值是数组时不要对数组本身写 `referenceFrame`,直接交给 `createEntityFromCzmlPosition`。

```javascript
const MU_MOON = 4.9028003055554e12;
const radius = 1737400 + 100000;
const speed = Math.sqrt(MU_MOON / radius);
const epoch = Cesium.JulianDate.fromIso8601("2022-06-24T20:00:00Z");
const stop = Cesium.JulianDate.addSeconds(epoch, 12 * 3600, new Cesium.JulianDate());

const propagator = new Cesium.TwoBodyPropagator(
    epoch,
    new Cesium.Cartesian3(radius, 0, 0),
    new Cesium.Cartesian3(0, 0, speed),
    MU_MOON
);
const packet = propagator.toCzmlPosition(epoch, stop, 60).getCzmlPostions();
if (!Array.isArray(packet)) {
    packet.referenceFrame = "INERTIAL";
}

const visual = {
    id: "moon-twobody-orbit",
    name: "月球二体轨道",
    centralBody: "Moon",
    path: {
        show: true,
        width: 2,
        resolution: 60,
        leadTime: 1e10,
        trailTime: 1e10,
        material: Cesium.Color.YELLOW
    },
    point: {
        pixelSize: 8,
        color: Cesium.Color.YELLOW,
        outlineColor: Cesium.Color.WHITE,
        outlineWidth: 1
    },
    label: {
        text: "月球二体轨道",
        font: "16px Microsoft YaHei",
        fillColor: Cesium.Color.WHITE,
        outlineColor: Cesium.Color.BLACK,
        outlineWidth: 2,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        pixelOffset: new Cesium.Cartesian2(0, -22)
    }
};

const existing = czmlDataSource.entities.getById(visual.id);
if (existing) {
    czmlDataSource.entities.remove(existing);
}
czmlDataSource.entities.add(createEntityFromCzmlPosition(packet, visual));
```

加入后把 `viewer.clock` 的 `startTime`、`stopTime`、`currentTime` 对齐到这段历元,`clockRange` 用 `Cesium.ClockRange.LOOP_STOP`。有 timeline 时再 `zoomTo(start, stop)`。

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
- 行星走 `Planetary.getPlanetary` + `solarSystem.addEntity`。轨迹先备好 `Cesium.CzmlPosition`(或同样字段的包 / 包数组),再 `createEntityFromCzmlPosition`,最后 `czmlDataSource.entities.add`。Entity 选项带 `centralBody`,不要在构造参数里写 `position`。
- 地面轨迹写在主 Entity 的 `path.groundTracks`,项为 `{ centralBody, show }`。月球表面用 `"Moon"`。多条相关轨迹赋给 `entity.orbits = new Cesium.OrbitsGraphics(...)`,`system` 用 `"Inertial"`,`positionType` 用 `"CzmlPosition"`。
- 影像、地形挂到天体名 `"Earth"` 或 `"Moon"`,极区必须带 `pole`,椭球与天体一致。
- 月球南极示例的影像服务是 `astrox.cn:8767`,地形服务是 `astrox.cn:8766`。换数据集时只替换 `url`、`maximumLevel` 与 `pole`,不要改调用顺序。
- 本文未出现的 ASTROX Cesium API 先查 `examples/` 与 `notes/`,没有则向用户确认,不要按原版 Cesium 猜测扩展参数。

## 标准执行流程

1. 确认问题是否涉及 ASTROX 扩展 Cesium。多天体场景、行星实体、轨迹 Entity、地面轨迹、orbits、VGT、地球或月球影像/地形属于本技能。
2. 场景创建按「初始化顺序」和「场景内推荐调用顺序」写,参数用本文表格与代码中的值。先完成必选 AstroxWasm,再装可选的 VGT 与 Planetary。
3. 图层按「影像图层与地形图层」选择天体。月球南极用已给出的 URL、`Ellipsoid.MOON` 和 `pole: "South"`。地球 OSM 按需添加并同步 `cloneObjects`。
4. 轨迹 Entity 按「用 CZML position 创建轨迹 Entity」写。已有 `CzmlPosition` 或同样字段的包时直接 `createEntityFromCzmlPosition`。地面轨迹与多条相关轨迹按「地月转移:地面轨迹与 orbits」写。月球二体先 `TwoBodyPropagator.toCzmlPosition(...).getCzmlPostions()`,`centralBody` 为 `"Moon"`,再加入 `czmlDataSource.entities`。
5. 本文没有的 API 或地形地址,说明资料尚未入库,不要编造。
