namespace utils {
    export const saveFile = (filename: string, data: string) => {
        const blob = new Blob([data], { type: 'text/plain' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
        a.remove();
    }

    export const copytoClipboard = (value: string) => {
        navigator.clipboard.writeText(value);
    }
}