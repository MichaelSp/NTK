export function toTime(seconds) {
    let date = new Date(null);
    date.setSeconds(seconds);
    return date.toISOString().substring(11, 19);
}