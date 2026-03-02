function refreshSensorsList() {
    var payload = {};
    payload.method_to_start = 'refreshSensorList';
    sendPayloadToPlugin(payload);
}