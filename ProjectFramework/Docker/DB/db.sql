--
-- PostgreSQL database dump
--

-- Dumped from database version 10.6
-- Dumped by pg_dump version 11.1

-- Started on 2019-01-21 19:20:24

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 2891 (class 1262 OID 16393)
-- Name: projectframework; Type: DATABASE; Schema: -; Owner: postgres
--

CREATE DATABASE projectframework WITH TEMPLATE = template0 ENCODING = 'UTF8';


ALTER DATABASE projectframework OWNER TO postgres;

\connect projectframework

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 196 (class 1259 OID 16394)
-- Name: group; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."group" (
    groupnumber integer NOT NULL
);


ALTER TABLE public."group" OWNER TO postgres;

--
-- TOC entry 197 (class 1259 OID 16397)
-- Name: group_groupnumber_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.group_groupnumber_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.group_groupnumber_seq OWNER TO postgres;

--
-- TOC entry 2892 (class 0 OID 0)
-- Dependencies: 197
-- Name: group_groupnumber_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.group_groupnumber_seq OWNED BY public."group".groupnumber;


--
-- TOC entry 198 (class 1259 OID 16399)
-- Name: user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."user" (
    name text NOT NULL,
    id integer NOT NULL
);


ALTER TABLE public."user" OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 16405)
-- Name: headofstudy; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.headofstudy (
    department text
)
INHERITS (public."user");


ALTER TABLE public.headofstudy OWNER TO postgres;

--
-- TOC entry 207 (class 1259 OID 16474)
-- Name: project; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.project (
    id integer NOT NULL,
    title text NOT NULL,
    "desc" text NOT NULL,
    teacher integer NOT NULL,
    prerequisites text[]
);


ALTER TABLE public.project OWNER TO postgres;

--
-- TOC entry 208 (class 1259 OID 16494)
-- Name: projectCosupervisor; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."projectCosupervisor" (
    teacher integer NOT NULL,
    project integer NOT NULL
);


ALTER TABLE public."projectCosupervisor" OWNER TO postgres;

--
-- TOC entry 206 (class 1259 OID 16472)
-- Name: project_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.project_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.project_id_seq OWNER TO postgres;

--
-- TOC entry 2893 (class 0 OID 0)
-- Dependencies: 206
-- Name: project_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.project_id_seq OWNED BY public.project.id;


--
-- TOC entry 205 (class 1259 OID 16461)
-- Name: restriction; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.restriction (
    type integer NOT NULL,
    n integer,
    project integer NOT NULL
);


ALTER TABLE public.restriction OWNER TO postgres;

--
-- TOC entry 204 (class 1259 OID 16450)
-- Name: restrictionType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."restrictionType" (
    "desc" text NOT NULL,
    id integer NOT NULL
);


ALTER TABLE public."restrictionType" OWNER TO postgres;

--
-- TOC entry 203 (class 1259 OID 16448)
-- Name: restrictionType_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."restrictionType_id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public."restrictionType_id_seq" OWNER TO postgres;

--
-- TOC entry 2894 (class 0 OID 0)
-- Dependencies: 203
-- Name: restrictionType_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."restrictionType_id_seq" OWNED BY public."restrictionType".id;


--
-- TOC entry 200 (class 1259 OID 16411)
-- Name: user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_id_seq OWNER TO postgres;

--
-- TOC entry 2895 (class 0 OID 0)
-- Dependencies: 200
-- Name: user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_id_seq OWNED BY public."user".id;


--
-- TOC entry 201 (class 1259 OID 16413)
-- Name: student; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.student (
    name text,
    id integer DEFAULT nextval('public.user_id_seq'::regclass),
    studynumber text NOT NULL,
    "group" integer
)
INHERITS (public."user");


ALTER TABLE public.student OWNER TO postgres;

--
-- TOC entry 202 (class 1259 OID 16420)
-- Name: teacher; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.teacher (
    email text
)
INHERITS (public."user");


ALTER TABLE public.teacher OWNER TO postgres;

--
-- TOC entry 2714 (class 2604 OID 16426)
-- Name: group groupnumber; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."group" ALTER COLUMN groupnumber SET DEFAULT nextval('public.group_groupnumber_seq'::regclass);


--
-- TOC entry 2716 (class 2604 OID 16427)
-- Name: headofstudy id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.headofstudy ALTER COLUMN id SET DEFAULT nextval('public.user_id_seq'::regclass);


--
-- TOC entry 2720 (class 2604 OID 16477)
-- Name: project id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project ALTER COLUMN id SET DEFAULT nextval('public.project_id_seq'::regclass);


--
-- TOC entry 2719 (class 2604 OID 16453)
-- Name: restrictionType id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."restrictionType" ALTER COLUMN id SET DEFAULT nextval('public."restrictionType_id_seq"'::regclass);


--
-- TOC entry 2718 (class 2604 OID 16428)
-- Name: teacher id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher ALTER COLUMN id SET DEFAULT nextval('public.user_id_seq'::regclass);


--
-- TOC entry 2715 (class 2604 OID 16429)
-- Name: user id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user" ALTER COLUMN id SET DEFAULT nextval('public.user_id_seq'::regclass);


--
-- TOC entry 2873 (class 0 OID 16394)
-- Dependencies: 196
-- Data for Name: group; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."group" (groupnumber) FROM stdin;
\.


--
-- TOC entry 2876 (class 0 OID 16405)
-- Dependencies: 199
-- Data for Name: headofstudy; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.headofstudy (name, id, department) FROM stdin;
\.


--
-- TOC entry 2884 (class 0 OID 16474)
-- Dependencies: 207
-- Data for Name: project; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.project (id, title, "desc", teacher, prerequisites) FROM stdin;
\.


--
-- TOC entry 2885 (class 0 OID 16494)
-- Dependencies: 208
-- Data for Name: projectCosupervisor; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."projectCosupervisor" (teacher, project) FROM stdin;
\.


--
-- TOC entry 2882 (class 0 OID 16461)
-- Dependencies: 205
-- Data for Name: restriction; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.restriction (type, n, project) FROM stdin;
\.


--
-- TOC entry 2881 (class 0 OID 16450)
-- Dependencies: 204
-- Data for Name: restrictionType; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."restrictionType" ("desc", id) FROM stdin;
Max Groups	1
Max Group Size	2
\.


--
-- TOC entry 2878 (class 0 OID 16413)
-- Dependencies: 201
-- Data for Name: student; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.student (name, id, studynumber, "group") FROM stdin;
\.


--
-- TOC entry 2879 (class 0 OID 16420)
-- Dependencies: 202
-- Data for Name: teacher; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.teacher (name, id, email) FROM stdin;
\.


--
-- TOC entry 2875 (class 0 OID 16399)
-- Dependencies: 198
-- Data for Name: user; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."user" (name, id) FROM stdin;
\.


--
-- TOC entry 2896 (class 0 OID 0)
-- Dependencies: 197
-- Name: group_groupnumber_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.group_groupnumber_seq', 1, false);


--
-- TOC entry 2897 (class 0 OID 0)
-- Dependencies: 206
-- Name: project_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.project_id_seq', 1, false);


--
-- TOC entry 2898 (class 0 OID 0)
-- Dependencies: 203
-- Name: restrictionType_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."restrictionType_id_seq"', 2, true);


--
-- TOC entry 2899 (class 0 OID 0)
-- Dependencies: 200
-- Name: user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_id_seq', 1, false);


--
-- TOC entry 2735 (class 2606 OID 16460)
-- Name: restrictionType desc_unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."restrictionType"
    ADD CONSTRAINT desc_unique UNIQUE ("desc");


--
-- TOC entry 2722 (class 2606 OID 16431)
-- Name: group groupnumber_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."group"
    ADD CONSTRAINT groupnumber_pkey PRIMARY KEY (groupnumber);


--
-- TOC entry 2726 (class 2606 OID 16433)
-- Name: headofstudy headOfStudy_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.headofstudy
    ADD CONSTRAINT "headOfStudy_pkey" PRIMARY KEY (id);


--
-- TOC entry 2724 (class 2606 OID 16435)
-- Name: user pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT pkey PRIMARY KEY (id);


--
-- TOC entry 2745 (class 2606 OID 16498)
-- Name: projectCosupervisor projectCosupervisor_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."projectCosupervisor"
    ADD CONSTRAINT "projectCosupervisor_pkey" PRIMARY KEY (teacher, project);


--
-- TOC entry 2743 (class 2606 OID 16482)
-- Name: project project_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project
    ADD CONSTRAINT project_pkey PRIMARY KEY (id);


--
-- TOC entry 2737 (class 2606 OID 16458)
-- Name: restrictionType restrictionType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."restrictionType"
    ADD CONSTRAINT "restrictionType_pkey" PRIMARY KEY (id);


--
-- TOC entry 2741 (class 2606 OID 16465)
-- Name: restriction restriction_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.restriction
    ADD CONSTRAINT restriction_pkey PRIMARY KEY (type, project);


--
-- TOC entry 2729 (class 2606 OID 16437)
-- Name: student student_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.student
    ADD CONSTRAINT student_pkey PRIMARY KEY (id);


--
-- TOC entry 2731 (class 2606 OID 16439)
-- Name: student studynumber_unqiue; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.student
    ADD CONSTRAINT studynumber_unqiue UNIQUE (studynumber);


--
-- TOC entry 2733 (class 2606 OID 16441)
-- Name: teacher teacher_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.teacher
    ADD CONSTRAINT teacher_pkey PRIMARY KEY (id);


--
-- TOC entry 2727 (class 1259 OID 16442)
-- Name: fki_group_fkey; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX fki_group_fkey ON public.student USING btree ("group");


--
-- TOC entry 2738 (class 1259 OID 16493)
-- Name: fki_project_fkey; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX fki_project_fkey ON public.restriction USING btree (project);


--
-- TOC entry 2739 (class 1259 OID 16471)
-- Name: fki_type_fkey; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX fki_type_fkey ON public.restriction USING btree (type);


--
-- TOC entry 2746 (class 2606 OID 16443)
-- Name: student group_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.student
    ADD CONSTRAINT group_fkey FOREIGN KEY ("group") REFERENCES public."group"(groupnumber);


--
-- TOC entry 2748 (class 2606 OID 16488)
-- Name: restriction project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.restriction
    ADD CONSTRAINT project_fkey FOREIGN KEY (project) REFERENCES public.project(id);


--
-- TOC entry 2751 (class 2606 OID 16504)
-- Name: projectCosupervisor project_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."projectCosupervisor"
    ADD CONSTRAINT project_fkey FOREIGN KEY (project) REFERENCES public.project(id);


--
-- TOC entry 2749 (class 2606 OID 16483)
-- Name: project teacher_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project
    ADD CONSTRAINT teacher_fkey FOREIGN KEY (teacher) REFERENCES public.teacher(id);


--
-- TOC entry 2750 (class 2606 OID 16499)
-- Name: projectCosupervisor teacher_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."projectCosupervisor"
    ADD CONSTRAINT teacher_fkey FOREIGN KEY (teacher) REFERENCES public.teacher(id);


--
-- TOC entry 2747 (class 2606 OID 16466)
-- Name: restriction type_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.restriction
    ADD CONSTRAINT type_fkey FOREIGN KEY (type) REFERENCES public."restrictionType"(id);


-- Completed on 2019-01-21 19:20:24

--
-- PostgreSQL database dump complete
--

