#!/usr/bin/env python3
"""Recursive ASM-call-graph + C#-port-status analyzer for the 0x91A0 campaign.

ASM<->cs1 rule: cs1_off = DNCDPRG.ASM_seg000_off - 0x1ED0.
Reports, for each reachable sub_XXXX, its size, call/jmp targets, and whether
the cs1 address is already a real C# override (DefineFunction + method across
ALL Overrides/*.cs) — never a hand-enumerated subset.
"""
import os, re, sys, subprocess

ASM = "OpenRakis/asm/cd/DNCDPRG.ASM"
OVR = "src/Cryogenic/Overrides"
DELTA = 0x1ED0

def load_procs():
    procs = {}  # name -> (start_line, end_line, body_lines)
    cur = None
    buf = []
    start = 0
    with open(ASM, encoding="latin-1") as f:
        lines = f.readlines()
    for i, ln in enumerate(lines):
        m = re.match(r'^(sub_[0-9A-F]+)\s+proc\s', ln)
        if m:
            cur = m.group(1); buf = [ln]; start = i; continue
        if cur:
            buf.append(ln)
            if re.match(r'^' + re.escape(cur) + r'\s+endp', ln):
                procs[cur] = (start, i, buf); cur = None; buf = []
    return procs

def cs1_of(name):
    return int(name[4:], 16) - DELTA

def targets(body):
    calls, jmps = [], []
    for ln in body:
        m = re.search(r'\bcall\s+(sub_[0-9A-F]+)', ln)
        if m: calls.append(m.group(1))
        m = re.search(r'\bjmp\s+(sub_[0-9A-F]+)', ln)
        if m: jmps.append(m.group(1))
        if re.search(r'call\s+(word|dword) ptr', ln): calls.append("<indirect>")
        if re.search(r'call\s+ax\b|call\s+bx\b', ln): calls.append("<reg-indirect>")
    return calls, jmps

def ported(cs1):
    h = "0x%X" % cs1
    try:
        reg = subprocess.run(["grep","-rl","DefineFunction(cs1, %s," % h, OVR],
                              capture_output=True, text=True).stdout.strip()
    except Exception:
        reg = ""
    files = [os.path.basename(x) for x in reg.splitlines() if "StaticDefinitions" not in x]
    return files

def main():
    roots = sys.argv[1:] or ["sub_B070"]
    procs = load_procs()
    pcache = {}
    def is_ported(cs1):
        if cs1 not in pcache: pcache[cs1] = ported(cs1)
        return pcache[cs1]
    # Full recursive traversal of the UNPORTED frontier.
    seen = set()
    stack = list(roots)
    nodes = {}   # name -> (cs1, size, calls, jmps)
    while stack:
        n = stack.pop()
        if n in seen: continue
        seen.add(n)
        if n not in procs:
            nodes[n] = None; continue
        s,e,body = procs[n]
        cs1 = cs1_of(n)
        c,j = targets(body)
        nodes[n] = (cs1, e-s, c, j)
        for t in c+j:
            if t.startswith("sub_") and t not in seen and not is_ported(cs1_of(t)):
                stack.append(t)
    # Classify each unported node.
    def dep_unported(t):
        return t.startswith("sub_") and not is_ported(cs1_of(t))
    leaves, blocked, indir = [], [], []
    for n, info in nodes.items():
        if info is None: continue
        cs1, sz, c, j = info
        if is_ported(cs1): continue
        has_ind = any(not x.startswith("sub_") for x in c)
        ud = sorted({t for t in c+j if dep_unported(t)})
        if has_ind: indir.append((n,cs1,sz,ud))
        elif not ud: leaves.append((n,cs1,sz))
        else: blocked.append((n,cs1,sz,ud))
    print("=== 0x91A0 chain — full unported frontier ===")
    print("total unported reachable nodes: %d  (chunked/unlabeled: %d)\n"
          % (sum(1 for v in nodes.values() if v and not is_ported(v[0])),
             sum(1 for v in nodes.values() if v is None)))
    print("--- TRUE LEAVES (all deps C#/none, no indirect) — port these first ---")
    for n,cs1,sz in sorted(leaves, key=lambda x:x[2]):
        print("  %-12s cs1:0x%-5X L=%d" % (n,cs1,sz))
    print("\n--- INDIRECT (call word/dword ptr — §A/§B) ---")
    for n,cs1,sz,ud in sorted(indir, key=lambda x:x[2]):
        print("  %-12s cs1:0x%-5X L=%-3d unported-direct-deps=%s" % (n,cs1,sz,ud or "[]"))
    print("\n--- BLOCKED (waiting on unported direct deps) ---")
    for n,cs1,sz,ud in sorted(blocked, key=lambda x:len(x[3])):
        print("  %-12s cs1:0x%-5X L=%-3d -> %s" % (n,cs1,sz," ".join(ud)))
    print("\n--- chunked/unlabeled (no proc/endp) ---")
    for n,info in nodes.items():
        if info is None: print("  %s" % n)

if __name__ == "__main__":
    main()
