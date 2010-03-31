%defaults in revision 599
% ts=0.0082 s - time between interrupts on PIC
% minfreq = 0.1 Hz
% maxfreq = 50 Hz
function do_bode(req, resp, ts, minfreq, maxfreq)

figure;
dt = iddata(req, resp, ts);
fdt = spafdr(dt, [], {2*pi*minfreq, 2*pi*maxfreq});
bode(fdt);
