// 多段 CzmlPositions: 按时间段为 entity.path 上色 (原版 Cesium TimeIntervalCollectionProperty)
// 仅影响 Entity.path,不影响 OrbitsGraphics。完整说明见 ../SKILL.md「多段 path 按时间上色」。

const SEGMENT_COLORS = [Cesium.Color.CYAN, Cesium.Color.ORANGE, Cesium.Color.LIME];

function splitIntervalString(interval) {
    const [start, stop] = String(interval || "").split("/");
    return { start: start || "", stop: stop || "" };
}

function intervalBounds(packet) {
    if (Array.isArray(packet)) {
        const first = splitIntervalString(packet[0]?.interval || packet[0]?.Interval);
        const last = splitIntervalString(packet[packet.length - 1]?.interval || packet[packet.length - 1]?.Interval);
        return { start: first.start, stop: last.stop };
    }
    const bounds = splitIntervalString(packet?.interval || packet?.Interval);
    return { start: bounds.start, stop: bounds.stop };
}

function collectSegmentBounds(interval) {
    if (!interval) return [];
    if (interval instanceof Cesium.TimeIntervalCollection) {
        const list = [];
        for (let i = 0; i < interval.length; i += 1) {
            const item = interval.get(i);
            list.push({
                start: Cesium.JulianDate.toIso8601(item.start),
                stop: Cesium.JulianDate.toIso8601(item.stop)
            });
        }
        return list;
    }

    const source = Array.isArray(interval) ? interval : [interval];
    return source.map((item) => {
        if (typeof item === "string") {
            const [start, stop] = item.split("/");
            return { start, stop };
        }
        if (item?.interval || item?.Interval) {
            return intervalBounds(item);
        }
        return {
            start: item?.start || item?.Start || "",
            stop: item?.stop || item?.Stop || ""
        };
    }).filter((item) => item.start && item.stop);
}

function getMultipleSegmentColors(entity, interval) {
    const segmentBounds = collectSegmentBounds(interval);
    if (!entity || segmentBounds.length === 0) return;

    if (!entity.path) {
        entity.path = new Cesium.PathGraphics({ show: true, width: 2 });
    }

    const colorProperty = new Cesium.TimeIntervalCollectionProperty();
    segmentBounds.forEach((segment, index) => {
        const start = Cesium.JulianDate.fromIso8601(segment.start);
        const stop = Cesium.JulianDate.fromIso8601(segment.stop);
        if (!start || !stop || Cesium.JulianDate.greaterThanOrEquals(start, stop)) return;
        colorProperty.intervals.addInterval(new Cesium.TimeInterval({
            start,
            stop,
            isStartIncluded: true,
            isStopIncluded: index === segmentBounds.length - 1,
            data: SEGMENT_COLORS[index % SEGMENT_COLORS.length]
        }));
    });

    if (colorProperty.intervals.isEmpty) return;

    const material = new Cesium.ColorMaterialProperty(colorProperty);
    entity.path.material = material;
    return material;
}

// 调用示例 (createEntityFromCzmlPosition 见 SKILL.md)
// const entity = createEntityFromCzmlPosition(packets, visual);
// getMultipleSegmentColors(entity, packets);
